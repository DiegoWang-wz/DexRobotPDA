let globalKeyPressHandler = null;

window.registerGlobalKeyPress = (dotNetRef, methodName) => {
    globalKeyPressHandler = (event) => {
        dotNetRef.invokeMethodAsync(methodName, event.key);
    };
    document.addEventListener('keydown', globalKeyPressHandler);
};

window.unregisterGlobalKeyPress = () => {
    if (globalKeyPressHandler) {
        document.removeEventListener('keydown', globalKeyPressHandler);
        globalKeyPressHandler = null;
    }
};