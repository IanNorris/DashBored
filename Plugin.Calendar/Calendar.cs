using DashBored.MicrosoftGraph;
using DashBored.PluginApi;
using Microsoft.Graph;
using Plugin.Calendar.Models;

namespace Plugin.Calendar
{
	public class Calendar : IPlugin
	{
		public static Type DataType => typeof(CalendarData);
		public Type RazorType => typeof(CalendarView);
		public CardStyle CardStyle => new CardStyle
		{
			Classes = "calendar-card",
			Padding = true,
		};

		public IDictionary<int, int> TimerFrequencies => new Dictionary<int, int>
		{
			{ 0, 15 * 60 * 1000 }, //15m
		};

		public IEnumerable<Secret> Secrets => null;

		public IEnumerable<string> ScriptPaths => new List<string>
		{
			"fullcalendar/lib/main.js",
			"fullcalendar/lib/locales-all.js",
			"calendar.js",
		};

		public IEnumerable<string> StylesheetPaths => new List<string>
		{
			"fullcalendar/lib/main.css",
			"calendar.css",
		};

		public string Error { get; set; }

		public List<CalendarEvent> CalendarEvents { get; set; } = new List<CalendarEvent>();
		public Dictionary<string, string> CalendarNames { get; set; } = new Dictionary<string, string>();
		public List<FullCalendarEvent> FullCalendarEvents { get; set; } = new List<FullCalendarEvent>();

		public string Title { get; set; }

		public GraphError GraphError { get; set; }

		public IPlugin.OnDataChangedDelegate OnDataChanged { get; set; }

		public GraphAuthenticationProviderPublic.OnLoginPromptDelegate OnLoginPrompt { get; set; }

		public Calendar(CalendarData data, string title)
		{
			_data = data;
			Title = title;
		}

		public async Task Login()
		{
			await _client.AcquireToken(true);

			await OnTimer(0);
		}

		public async Task<bool> OnInitialize(IPluginSecrets pluginSecrets)
		{
			_client = new GraphClient(pluginSecrets, _data.AzureAD, _scopes, (errorType, message) =>
			{
				GraphError = errorType;
				Error = message;

				OnDataChanged?.Invoke();
			},
			async (targetUri, redirectUri, cancellationToken) =>
			{
				return await OnLoginPrompt(targetUri, redirectUri, cancellationToken);
			});

			await OnTimer(0);

			return true;
		}

		public async Task<bool> OnTimer(int _)
		{
			//Don't retry on FatalError or NeedsAuth.
			if(GraphError == GraphError.Success || GraphError == GraphError.Error)
			{
				try
				{
					var queryRange = new List<QueryOption>()
					{
						new QueryOption("startDateTime", DateTime.UtcNow.AddDays(-1).ToString("O")),
						new QueryOption("endDateTime", DateTime.UtcNow.AddDays(8).ToString("O")),
					};

					var calendars = await _client.Client.Me.Calendars.Request().GetAsync();

					var newCalendarEvents = new List<CalendarEvent>();
					var newCalendarNames = new Dictionary<string, string>();

					var orderedCalendars = calendars.OrderBy(c => c.Name);

					int colourIndex = 0;
					foreach (var calendar in orderedCalendars)
					{
						string color = null;
						if(_data.Calendars.TryGetValue(calendar.Name, out var calendarSettings))
						{
							color = calendarSettings.Color;
						}
						
						if(color == null)
						{
							color = _colours[(colourIndex++) % _colours.Length];
						}

						newCalendarNames.Add(calendar.Name, color);

						var calendarData = await _client.Client.Me.Calendars[calendar.Id].CalendarView.Request(queryRange).GetAsync();
						var calendarEvents = calendarData;

						if (calendarEvents != null)
						{
							var pageIterator = PageIterator<Event>.CreatePageIterator(
								_client.Client, calendarEvents,
								e =>
								{
									var newEvent = new CalendarEvent
									{
										Title = e.Subject,
										Color = color,
										StartTime = DateTime.Parse(e.Start.DateTime),
										EndTime = DateTime.Parse(e.End.DateTime),
										AllDay = e.IsAllDay.GetValueOrDefault(),
									};

									var existing = newCalendarEvents.FirstOrDefault(c =>
										  c.StartTime == newEvent.StartTime
									   && c.EndTime == newEvent.EndTime
									   && c.Title == newEvent.Title
									   && c.AllDay == newEvent.AllDay);

									if (existing == null)
									{
										newCalendarEvents.Add(newEvent);
									}

									return true;
								}
							);

							await pageIterator.IterateAsync();
						}
					}

					lock (CalendarEvents)
					{
						CalendarEvents.Clear();

						CalendarEvents.AddRange(newCalendarEvents);
					}

					lock(CalendarNames)
					{
						CalendarNames.Clear();

						foreach(var item in newCalendarNames)
						{
							CalendarNames.Add(item.Key, item.Value);
						}
					}

					lock(FullCalendarEvents)
					{
						FullCalendarEvents.Clear();

						foreach(var calendarEvent in CalendarEvents)
						{
							var newFullCalendarEvent = new FullCalendarEvent
							{
								Title = calendarEvent.Title,
								Start = calendarEvent.StartTime.ToLocalTime(),
								End = calendarEvent.EndTime.ToLocalTime(),
								ClassName = $"calendar-event-{calendarEvent.Color}",
								AllDay = calendarEvent.AllDay,
							};

							FullCalendarEvents.Add(newFullCalendarEvent);
						}
					}

					OnDataChanged?.Invoke();
				}
				catch
				{
					if(GraphError == GraphError.Success)
					{
						throw;
					}
				}
			}

			return true;
		}

		private GraphClient _client;
		private CalendarData _data;

		private string[] _colours = { "blue", "green", "red", "linen", "dgreen" };

		private string[] _scopes = { "email", "profile", "offline_access", "User.Read", "Calendars.Read", "Calendars.Read.Shared" };
	}
}
