import {setSelection, setupEditor} from "./editor";
import {createObjectURL, revokeObjectURL} from "./util";
import {initializeApplication} from "./application";

window.initializeApplication = initializeApplication;
window.setSelection = setSelection;
window.setupEditor = setupEditor;
window.createObjectUrl = createObjectURL;
window.revokeObjectUrl = revokeObjectURL;

declare global {
    interface Window {
        setupEditor: (id: string, view: any) => void;
        setSelection: (from: number, to: number) => void;
        createObjectUrl: (imageStream: any) => Promise<string>;
        revokeObjectUrl: (url: string) => Promise<void>;
        initializeApplication: (ref: any) => void;
    }
}