import {debounce} from "./util";

let isCtrlPressed = false;
let isShiftPressed = false;
let isAltPressed = false;
let isMetaPressed = false;

const typableCharacters: string[] = [];

for (let i = 32; i <= 126; i++) {
    typableCharacters.push(String.fromCharCode(i));
}


export function setupEditor(id: string, view: any) {
    const editor = document.getElementById(id);

    editor.addEventListener('paste', async (ev) => {
        ev.preventDefault();
        ev.stopPropagation();

        let items = ev.clipboardData.items;
        for (let i = 0; i < items.length; i++) {
            let item = items[i];

            if (item.type === 'text/plain') {
                const text = ev.clipboardData.getData('text');
                await view.invokeMethodAsync('HandleInput', "insertFromPaste", text, null, isCtrlPressed, isShiftPressed, isAltPressed, isMetaPressed);

                return;
            } else if (item.type.indexOf('image') !== -1) {
                let file = item.getAsFile();
                let arrayBuffer = await file.arrayBuffer();
                let bytes = new Uint8Array(arrayBuffer);

                await view.invokeMethodAsync('HandleInput', "insertFromPaste", "", bytes, isCtrlPressed, isShiftPressed, isAltPressed, isMetaPressed);
            }
        }
    });

    editor.addEventListener('beforeinput', async (ev) => {
        ev.preventDefault();
        ev.stopPropagation();

        if (ev.inputType === 'insertFromPaste')
            return;

        console.log("Before input", ev.inputType, ev.data);

        await view.invokeMethodAsync('HandleInput', ev.inputType, ev.data, null, isCtrlPressed, isShiftPressed, isAltPressed, isMetaPressed);
    })

    editor.addEventListener('keydown', async (ev) => {
        // Update modifier states on keydown
        isCtrlPressed = ev.ctrlKey;
        isShiftPressed = ev.shiftKey;
        isAltPressed = ev.altKey;
        isMetaPressed = ev.metaKey;

        if (ev.key === 'Alt')
            ev.preventDefault();

        if (!typableCharacters.includes(ev.key))
            return

        // create an array with every character we can type in the editor


        ev.preventDefault();
        ev.stopPropagation();
        console.log("Key up", ev);
        await view.invokeMethodAsync('HandleKeyUp', ev.key, ev.altKey, ev.ctrlKey, ev.shiftKey);
    });


    editor.addEventListener('keyup', async (ev) => {
        if (ev.key === 'Control') isCtrlPressed = false;
        if (ev.key === 'Shift') isShiftPressed = false;
        if (ev.key === 'Alt') isAltPressed = false;
        if (ev.key === 'Meta') isMetaPressed = false;
    });

    // TODO
    // maybe this should be handled by mouse up and key up, to ensure we dont get a selection change on mouse down
    // or we could include a small delay before we send the selection change
    document.addEventListener('selectionchange', debounce(async (ev: Event) => {

        let isInsideEditor = false;
        let node = window.getSelection().anchorNode;

        while (node != null) {
            if ((node as Element).id === id) {
                isInsideEditor = true;
                break;
            }
            node = node.parentNode;
        }
        if (isInsideEditor === false)
            return;

        ev.preventDefault();
        ev.stopPropagation();

        let anchorNode = window.getSelection().anchorNode;
        while (anchorNode.hasChildNodes()) {
            let nextNode = anchorNode.firstChild;
            while (nextNode && nextNode.nodeType === 8) { // 8 is the nodeType for comments
                nextNode = nextNode.nextSibling;
            }
            if (!nextNode) {
                break;
            }

            anchorNode = nextNode;
        }

        // @ts-ignore
        while (anchorNode.parentNode && (anchorNode as Element).attributes === undefined || (anchorNode as Element).attributes['from'] === undefined) {
            anchorNode = anchorNode.parentNode;
        }

        let focusNode = window.getSelection().focusNode;
        while (focusNode.hasChildNodes()) {
            let nextNode = focusNode.firstChild;
            while (nextNode && nextNode.nodeType === 8) { // 8 is the nodeType for comments
                nextNode = nextNode.nextSibling;
            }
            if (!nextNode) {
                break;
            }

            focusNode = nextNode;
        }

        // @ts-ignore
        while (focusNode.parentNode && (focusNode as Element).attributes === undefined || (focusNode as Element).attributes['from'] === undefined)
            focusNode = focusNode.parentNode;

        const selection = window.getSelection();
        let anchorOffset = selection.anchorOffset;

        // TODO make this possible for all nodes with a specific attribute like 'offset'
        if (anchorNode.lastChild.nodeName === 'BR' || anchorNode.lastChild.nodeName === 'HR')
            anchorOffset = 0;
        let focusOffset = selection.focusOffset;
        if (focusNode.lastChild.nodeName === 'BR' || focusNode.lastChild.nodeName === 'HR')
            focusOffset = 0;

        // @ts-ignore
        const fromOffset = anchorOffset + parseInt(anchorNode.attributes['from'].value, 10);
        // @ts-ignore
        const toOffset = focusOffset + parseInt(focusNode.attributes['from'].value, 10);

        await view.invokeMethodAsync('HandleSelectionChange', fromOffset, toOffset);
    }, 100));
}

export function setSelection(from: number, to: number) {
    const selection = window.getSelection();
    const range = selection.getRangeAt(0).cloneRange();

    // search for the node where from is inside the node attribute 'from' and 'to' 
    const nodes = document.querySelectorAll(`[from]`);
    let startNode = null;
    let startNodeOffset = 0;
    for (let i = 0; i < nodes.length; i++) {
        const node = nodes[i];
        // @ts-ignore
        const start = parseInt(node.attributes['from'].value, 10);
        // @ts-ignore
        const end = parseInt(node.attributes['to'].value, 10);
        if (from >= start && from <= end) {
            startNode = node;
            startNodeOffset = start;
            break;
        }
    }

    let endNode = null;
    let endNodeOffset = 0;
    for (let i = 0; i < nodes.length; i++) {
        const node = nodes[i];
        // @ts-ignore
        const start = parseInt(node.attributes['from'].value, 10);
        // @ts-ignore
        const end = parseInt(node.attributes['to'].value, 10);
        if (to >= start && to <= end) {
            endNode = node;
            endNodeOffset = start;
            break;
        }
    }

    console.log("Setting selection", startNode, endNode, from, to);
    console.log("getRawText(startNode)", getRawText(startNode));

    if (startNode && endNode) {
        try {
            range.setStart(getRawText(startNode), from - startNodeOffset);
            range.setEnd(getRawText(endNode), to - endNodeOffset);
            selection.removeAllRanges();
            selection.addRange(range);
        } catch (e) {
            console.log("Error setting selection", e);
        }
    } else {
        console.log("Could not find start or end node", startNode, endNode);
    }
}

function getRawText(node: Node) {
    let child = node.firstChild;
    while (child.nodeType === 8 && child.nextSibling) { // 8 is the nodeType for comments
        child = child.nextSibling;
    }

    while (child.nodeType !== 3 && child.firstChild) {
        child = child.firstChild;

    }

    return child;
}