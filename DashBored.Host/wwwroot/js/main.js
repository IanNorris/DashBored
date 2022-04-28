function openWindow(targetUri) {
	window.open(targetUri, '_blank', 'width=400,height=700,scrollbars=no,toolbar=no,location=no', false);
}

function sleep(delayMS) {
    return new Promise(p => window.setTimeout(p, delayMS));
}

async function onConnectionDropped(options) {
    document.getElementById("loaderRoot").classList = "";

    let retiesOnFailLeft = options.retriesOnFail;
    for (let i = 0; i < options.retries; i++) {
        try {
            const result = await window.Blazor.reconnect();
            if (!result) {
                retiesOnFailLeft--;
                if (retiesOnFailLeft == 0) {
                    //Something bad happened, try a full reload
                    location.reload();
                }
                else {
                    //Maybe server is starting up?
                    await sleep(options.retryOnFailMS);
                }
            }
        } catch (err) {
            await sleep(options.retryDelayMS);
        }
    }

    location.reload();
}

function onFinishedLoading() {
    document.getElementById("loaderRoot").classList = "hide";
}

function onConnectionRestored(e) {
    onFinishedLoading();
}

window.Blazor.start({
    "reconnectionOptions": {
        "retries": 1000,
        "retriesOnFail": 5,
        "retryDelayMS": 2000,
        "retryOnFailMS": 3000,
    },
    reconnectionHandler: {
        onConnectionDown: options => onConnectionDropped(options),
        onConnectionUp: options => onConnectionRestored(options)
    }
});
