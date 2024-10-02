// @ts-ignore
export function debounce(fn, delay) {
    let timer: any = null;
    return function () {
        let context = this, args = arguments;
        clearTimeout(timer);
        timer = setTimeout(function () {
            fn.apply(context, args);
        }, delay);
    };
}


export async function createObjectURL(imageStream: any) {
    const arrayBuffer = await imageStream.arrayBuffer();
    const blob = new Blob([arrayBuffer]);
    return URL.createObjectURL(blob);
}

export async function revokeObjectURL(url: string) {
    URL.revokeObjectURL(url);
}