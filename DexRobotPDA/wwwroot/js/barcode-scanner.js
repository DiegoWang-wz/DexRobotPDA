// 条码扫描器模块
window.BarcodeScanner = {
    currentBarcode: '',
    barcodeTimer: null,
    scanThreshold: 50,
    dotNetRef: null,
    inputElement: null,

    setup: function(dotNetRef, element) {
        try {
            this.dotNetRef = dotNetRef;
            this.inputElement = element;

            if (!element) {
                throw new Error('未找到输入元素');
            }

            // 忽略回车键默认行为
            element.addEventListener('keydown', (e) => {
                if (e.key === 'Enter') {
                    e.preventDefault();
                    return false;
                }
            });

            // 处理按键释放事件
            element.addEventListener('keyup', (e) => {
                if (e.key === 'Enter') {
                    this.processCompleteScan();
                }
                // 处理可打印字符
                else if (e.key.length === 1 && e.key.charCodeAt(0) >= 32 && e.key.charCodeAt(0) <= 126) {
                    this.handleCharacterInput(e.key);
                }
            });
        } catch (error) {
            console.error('初始化错误:', error);
            this.logToBlazor(`扫码枪错误: ${error.message}`);
        }
    },

    handleCharacterInput: function(char) {
        if (this.barcodeTimer) clearTimeout(this.barcodeTimer);

        this.currentBarcode += char;

        this.barcodeTimer = setTimeout(() => {
            this.processCompleteScan();
        }, this.scanThreshold);
    },

    processCompleteScan: function() {
        if (this.currentBarcode && this.currentBarcode.length > 0) {
            if (this.dotNetRef) {
                this.dotNetRef.invokeMethodAsync('ReceiveBarcode', this.currentBarcode)
                    .catch(() => {
                        this.logToBlazor('传递扫描结果失败');
                    });
            }
        }

        // 重置状态 - 移除自动聚焦逻辑，由Blazor控制
        this.currentBarcode = '';
        if (this.inputElement) this.inputElement.value = '';
        if (this.barcodeTimer) {
            clearTimeout(this.barcodeTimer);
            this.barcodeTimer = null;
        }
    },

    logToBlazor: function(message) {
        if (this.dotNetRef) {
            this.dotNetRef.invokeMethodAsync('LogFromJS', message).catch(() => {});
        }
    },

    cleanup: function() {
        if (this.inputElement) {
            // 移除事件监听器
            this.inputElement.removeEventListener('keydown', () => {});
            this.inputElement.removeEventListener('keyup', () => {});
        }
        if (this.barcodeTimer) clearTimeout(this.barcodeTimer);
        this.dotNetRef = null;
        this.inputElement = null;
        this.currentBarcode = '';
    }
};
