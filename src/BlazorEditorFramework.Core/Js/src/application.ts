export function initializeApplication(ref: any) {
    /*let app = document.getElementById("application");*/

    document.addEventListener(`keyup`, async (ev) => {
        console.log("keypress application", ev);

        await ref.invokeMethodAsync('HandleInput', ev.key, ev.ctrlKey, ev.shiftKey, ev.altKey);
    });

}