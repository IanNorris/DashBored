using System.IO.Compression;
using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;
using DashBored.TradingView.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DashBored.TradingView
{
	public class TVWebSocket : IDisposable
	{
		public delegate void OnQuoteUpdateDelegate(Quote quoteData);

		public TVWebSocket()
		{
			_reader = new StreamReader(_stream);

			_webSocket = new ClientWebSocket();

			_cancellationToken = new CancellationTokenSource();

			_thread = new Thread(() => ThreadMain());
			_thread.Start();

			_sendThread = new Thread(() => SendThreadMain());
		}

		private async Task Connect()
		{
			_webSocket.Options.SetRequestHeader("origin", Origin);
			await _webSocket.ConnectAsync(new Uri(EndpointUri), _cancellationToken.Token);
		}

		public void ThreadMain()
		{
			Main().GetAwaiter().GetResult();
		}

		public void SendThreadMain()
		{
			SendMain().GetAwaiter().GetResult();
		}

		private async Task SendMain()
		{
			List<byte[]> packetsToSend = null;

			while (_webSocket.State == WebSocketState.Open && !_cancellationToken.IsCancellationRequested)
			{

				lock (_sendPackets)
				{
					packetsToSend = new List<byte[]>(_sendPackets);
					_sendPackets.Clear();
				}

				foreach (var packet in packetsToSend)
				{
					await _webSocket.SendAsync(packet, WebSocketMessageType.Text, true, _cancellationToken.Token);
				}
			}
		}

		private async Task Main()
		{
			int packetsProcessed = 0;

			await Connect();

			var Buffer = new byte[256 * 1024];

			while(_webSocket.State == WebSocketState.Open && !_cancellationToken.IsCancellationRequested)
			{
				var result = await _webSocket.ReceiveAsync(Buffer, _cancellationToken.Token);
				if(result != null)
				{
					_stream.Write(Buffer, 0, result.Count);

					if (result.EndOfMessage)
					{
						_stream.Position = 0;

						var bufferText = _reader.ReadToEnd();

						while (bufferText.Length > 0)
						{
							if (bufferText.Length < 3 || bufferText[0] != '~')
							{
								throw new InvalidDataException($"Unable to process packet: {bufferText}");
							}

							var headerMatch = ParseHeader.Match(bufferText);
							if (!headerMatch.Success)
							{
								throw new InvalidDataException($"Unable to process packet header: {bufferText}");
							}

							var byteLength = int.Parse(headerMatch.Groups[1].Value);
							var headerLength = headerMatch.Groups[0].Length;

							var payload = bufferText.Substring(headerLength, byteLength);

							bool processed = false;
							if (payload.Length > 0)
							{
								if (payload[0] == '{')
								{
									var payloadTokens = JObject.Parse(payload);
									if (payloadTokens.ContainsKey("m"))
									{
										var packetType = payloadTokens["m"].Value<string>();
										var parameters = payloadTokens["p"];
										switch (packetType)
										{
											case "qsd":
												var sessionId = parameters[0].Value<string>();
												var quote = parameters[1].ToObject<Quote>();

												if (quote.Status == "ok")
												{
													if (_sessions.TryGetValue(sessionId, out var sessionHandler))
													{
														sessionHandler(quote);
													}

													processed = true;
												}
												else
												{
													Console.Error.WriteLine($"Error \"{quote.Status}\" with quote: {quote.Name}");
													processed = true;
												}

												break;

											case "quote_completed":
												processed = true; //think its ok to just continue to receive these?
												break;

											default:
												Console.Error.WriteLine($"Unknown packet \"{packetType}\" with payload: ");
												break;

										}
									}
									else
									{
										if(payloadTokens.ContainsKey("protocol"))
										{
											//Ignore it, its the welcome message
											processed = true;
										}
									}
								}
								else if(payload[0] == '~')
								{
									//Send a pong response
									if (payload.StartsWith("~h~"))
									{
										QueuePacket(payload);
										processed = true;
									}
								}
							}
							bufferText = bufferText.Substring(headerLength + byteLength);

							if (!processed)
							{
								Console.Out.WriteLine(payload);
							}

							packetsProcessed++;

							if(packetsProcessed == 1)
							{
								_sendThread.Start();

								QueueMethod("set_auth_token", "unauthorized_user_token");
							}
						}
					}
				}

				if(_cancellationToken.IsCancellationRequested)
				{
					break;
				}
			}
		}

		public void QueuePacket(string content)
		{
			var buffer = $"~m~{content.Length}~m~{content}";

			var bytes = Encoding.UTF8.GetBytes(buffer);

			lock (_sendPackets)
			{
				_sendPackets.Add(bytes);
			}
		}

		public void QueuePacket(object content)
		{
			var bufferString = JsonConvert.SerializeObject(content);

			QueuePacket(bufferString);
		}

		public void QueueMethod(string name, params string[] parameters)
		{
			var bufferString = JsonConvert.SerializeObject(new Method(name, parameters));

			QueuePacket(bufferString);
		}

		public void Dispose()
		{
			_cancellationToken.Cancel();
			_sendThread.Join();
			_thread.Join();
		}

		public string CreateQuoteSession(OnQuoteUpdateDelegate onQuoteUpdate, string [] fields)
		{
			var sessionId = Guid.NewGuid().ToString().Replace("-", "");

			QueueMethod("quote_create_session", sessionId);

			var newParameters = new List<string>(fields.Length + 1);
			newParameters.Add(sessionId);
			newParameters.AddRange(fields);

			QueueMethod("quote_set_fields", newParameters.ToArray());

			_sessions.Add(sessionId, onQuoteUpdate);

			return sessionId;
		}

		public void AddSymbol(string quoteSession, string ticker)
		{
			QueueMethod("quote_add_symbols", quoteSession, ticker);
		}

		public void RemoveSymbol(string quoteSession, string ticker)
		{
			QueueMethod("quote_remove_symbols", quoteSession, ticker);
		}

		private const string EndpointUri = "wss://data.tradingview.com/socket.io/websocket";
		private const string Origin = "https://www.tradingview.com";

		private readonly Regex ParseHeader = new Regex(@"~m~(\d+)~m~", RegexOptions.Compiled);

		private CancellationTokenSource _cancellationToken;
		private ClientWebSocket _webSocket;
		private Thread _thread;
		private Thread _sendThread;

		private MemoryStream _stream = new MemoryStream();
		private StreamReader _reader;

		private List<byte[]> _sendPackets = new List<byte[]>();

		private Dictionary<string, OnQuoteUpdateDelegate> _sessions = new Dictionary<string, OnQuoteUpdateDelegate>();
	}
}