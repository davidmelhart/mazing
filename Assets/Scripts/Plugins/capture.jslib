var CapturePlugin = {
    InitRecord: function() {
        window.initRecord();
    },
    StartRecord: function() {
        window.startRecord();
    },
    StopRecord: function() {
        window.stopRecord();
    },
};
 
mergeInto(LibraryManager.library, CapturePlugin);