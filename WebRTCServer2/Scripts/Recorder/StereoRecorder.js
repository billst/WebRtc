// Muaz Khan     - https://github.com/muaz-khan 
// neizerth      - https://github.com/neizerth
// MIT License   - https://www.webrtc-experiment.com/licence/
// Documentation - https://github.com/streamproc/MediaStreamRecorder
// ==========================================================
// StereoRecorder.js

function StereoRecorder(mediaStream, mediaStreamRemote) {
    // void start(optional long timeSlice)
    // timestamp to fire "ondataavailable"
    this.start = function (timeSlice) {
        timeSlice = timeSlice || 1000;

        mediaRecorder = new StereoAudioRecorder(mediaStream, this, mediaStreamRemote);

        (function looper() {
            mediaRecorder.record();

            setTimeout(function () {
                mediaRecorder.stop();
                looper();
            }, timeSlice);
        })();
    };

    this.stop = function () {
        if (mediaRecorder) mediaRecorder.stop();
    };

    this.ondataavailable = function () { };

    // Reference to "StereoAudioRecorder" object
    var mediaRecorder;
}

// source code from: http://typedarray.org/wp-content/projects/WebAudioRecorder/script.js
function StereoAudioRecorder(mediaStream, root, mediaStreamRemote) {
    // variables
    var leftchannel = [];
    var rightchannel = [];
    var recorder;
    var recording = false;
    var recordingLength = 0;
    var volume;
    var audioInput;
    var sampleRate = 44100;
    var audioContext;
    var context;

    this.record = function () {
        recording = true;
        // reset the buffers for the new recording
        leftchannel.length = rightchannel.length = 0;
        recordingLength = 0;
    };

    this.stop = function () {
        // we stop recording
        recording = false;

        // we flat the left and right channels down
        var leftBuffer = mergeBuffers(leftchannel, recordingLength);
        var rightBuffer = mergeBuffers(rightchannel, recordingLength);
        // we interleave both channels together
        var interleaved = interleave(leftBuffer, rightBuffer);

        // we create our wav file
        var buffer = new ArrayBuffer(44 + interleaved.length * 2);
        var view = new DataView(buffer);

        // RIFF chunk descriptor
        writeUTFBytes(view, 0, 'RIFF');
        view.setUint32(4, 44 + interleaved.length * 2, true);
        writeUTFBytes(view, 8, 'WAVE');
        // FMT sub-chunk
        writeUTFBytes(view, 12, 'fmt ');
        view.setUint32(16, 16, true);
        view.setUint16(20, 1, true);
        // stereo (2 channels)
        view.setUint16(22, 2, true);
        view.setUint32(24, sampleRate, true);
        view.setUint32(28, sampleRate * 4, true);
        view.setUint16(32, 4, true);
        view.setUint16(34, 16, true);
        // data sub-chunk
        writeUTFBytes(view, 36, 'data');
        view.setUint32(40, interleaved.length * 2, true);

        // write the PCM samples
        var lng = interleaved.length;
        var index = 44;
        var volume = 1;
        for (var i = 0; i < lng; i++) {
            view.setInt16(index, interleaved[i] * (0x7FFF * volume), true);
            index += 2;
        }

        // our final binary blob
        var blob = new Blob([view], { type: 'audio/wav' });
        console.log("StereoRecorder.js line 100, blob:", blob.size.toString());
        root.ondataavailable(blob);
    };

    function interleave(leftChannel, rightChannel) {
        var length = leftChannel.length + rightChannel.length;
        var result = new Float32Array(length);

        var inputIndex = 0;

        for (var index = 0; index < length;) {
            result[index++] = leftChannel[inputIndex];
            result[index++] = rightChannel[inputIndex];
            inputIndex++;
        }
        return result;
    }

    function mergeBuffers(channelBuffer, recordingLength) {
        var result = new Float32Array(recordingLength);
        var offset = 0;
        var lng = channelBuffer.length;
        for (var i = 0; i < lng; i++) {
            var buffer = channelBuffer[i];
            result.set(buffer, offset);
            offset += buffer.length;
        }
        return result;
    }

    function writeUTFBytes(view, offset, string) {
        var lng = string.length;
        for (var i = 0; i < lng; i++) {
            view.setUint8(offset + i, string.charCodeAt(i));
        }
    }

    // creates the audio context
    audioContext = window.AudioContext || window.webkitAudioContext;
    audioContextRemote = window.AudioContext || window.webKitAudioContext;

    context = new audioContext();

    contextRemote = new audioContextRemote();

    // creates a gain node
    volume = context.createGain();

    volumeremote = contextRemote.createGain();

    // creates an audio node from the microphone incoming stream
    audioInput = context.createMediaStreamSource(mediaStream);

    audioInputRemote = contextRemote.createMediaStreamSource(mediaStreamRemote);
    console.log("StreamRecorder.js line 154, mediaStreamRemote :", mediaStreamRemote.toString());
    

    // connect the stream to the gain node
    audioInput.connect(volume);

    audioInputRemote.connect(volumeremote);

    /* From the spec: This value controls how frequently the audioprocess event is 
    dispatched and how many sample-frames need to be processed each call. 
    Lower values for buffer size will result in a lower (better) latency. 
    Higher values will be necessary to avoid audio breakup and glitches */
    var bufferSize = 2048;
    this.recorderRemote = contextRemote.createScriptProcessor(bufferSize, 2, 2);
    this.recorder = context.createScriptProcessor(bufferSize, 2, 2);

 

   this.recorderRemote.onaudioprocess = function (e) {

       if (!recording) return;
       var right = e.inputBuffer.getChannelData(0);
       var float_right = new Float32Array(right);
       console.log("StreamRecorder.js line 174 float_right[1] - [100] : ", float_right[1], float_right[2], float_right[200]);
       rightchannel.push(float_right);
       recordingLength += bufferSize;
      console.log("StereoRecorder.js line 175, recordingLength: ", recordingLength.toString());
   }

   this.recorder.onaudioprocess = function (e) {
        if (!recording) return;
        var left = e.inputBuffer.getChannelData(0);
       // var right = e.inputBuffer.getChannelData(1);
       // we clone the samplesz
        var float_left = new Float32Array(left);
        console.log("StreamRecorder.js line 174 float_right[1] - [100] : ", float_left[1], float_left[2], float_left[200]);

        leftchannel.push(float_left);
       // rightchannel.push(new Float32Array(right));
        recordingLength += bufferSize;
       console.log("StereoRecorder.js line 186, recordingLength: ", recordingLength.toString());
    }; // we connect the recorder
   volume.connect(this.recorder);
   volumeremote.connect(this.recorderRemote);

   this.recorder.connect(context.destination);

    
    this.recorderRemote.connect(contextRemote.destination);
}