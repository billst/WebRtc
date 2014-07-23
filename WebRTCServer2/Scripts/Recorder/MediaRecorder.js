// Muaz Khan     - https://github.com/muaz-khan 
// neizerth      - https://github.com/neizerth
// MIT License   - https://www.webrtc-experiment.com/licence/
// Documentation - https://github.com/streamproc/MediaStreamRecorder
// ==========================================================
// MediaRecorder.js

function MediaRecorder(mediaStream) {
    // void start(optional long timeSlice)
    // timestamp to fire "ondataavailable"
    this.start = function (timeSlice) {
        timeSlice = timeSlice || 1000;
        console.log(" MediaRecorder.js line 13");
        mediaRecorder = new MediaRecorderWrapper(mediaStream);
        console.log(" MediaRecorder.js line 15");

        mediaRecorder.ondataavailable = function (e) {
            console.log(" MediaRecorder.js line inside 'mediaRecorder.ondataavailable' 18");

            console.log("MeidaRecorder.js line 20 blob.size:", e.data.size);
            console.log("MeidaRecorder.js line 21 blob.type:", e.data.type);
            console.log("MeidaRecorder.js line 22 blob.valueOf():", e.data.valueOf());
            if (mediaRecorder.state == 'recording') {
                var blob = e.data;
                //var blob = new window.Blob([e.data], {
                //    type: self.mimeType || 'audio/ogg'
           // }
               //)
                console.log("MeidaRecorder.js line 24 blob.size:", blob.size);
                console.log("MeidaRecorder.js line 25 blob.type:", blob.type);
                console.log("MeidaRecorder.js line 25 blob.valueOf():", blob.valueOf());
                self.ondataavailable(blob);
                mediaRecorder.stop();
            }
        };

        mediaRecorder.onstop = function () {
            if (mediaRecorder.state == 'inactive') {
                // bug: it is a temporary workaround; it must be fixed.
                mediaRecorder = new MediaRecorder(mediaStream);
                mediaRecorder.ondataavailable = self.ondataavailable;
                mediaRecorder.onstop = self.onstop;
                mediaRecorder.mimeType = self.mimeType;
                mediaRecorder.start(timeSlice);
            }

            self.onstop();
        };

        // void start(optional long timeSlice)
        mediaRecorder.start(timeSlice);
    };

    this.stop = function () {
        if (mediaRecorder && mediaRecorder.state == 'recording') {
            mediaRecorder.stop();
        }
    };

    this.ondataavailable = function () { };
    this.onstop = function () { };

    // Reference to itself
    var self = this;

    // Reference to "MediaRecorderWrapper" object
    var mediaRecorder;
}