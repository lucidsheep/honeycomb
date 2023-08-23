var WebSocketJavaScriptLibrary = {

/*
  $ConnectionOpen: [],
  $ConnectionClosed: [],
  $ReceivedByteArrayMessage: [],
  $ReceivedTextMessage: [],
  $ReceivedError: [],
  */
  $WebSocketCallbacks: [], //open, closed, onByteArrayMessage, onTextMessage, error
  $WebSocketArray: [],

  ConnectWebSocket: function init(socketID, wsUri) {
    var url = Pointer_stringify(wsUri);

    websocket = new WebSocket(url);
    websocket.binaryType = "arraybuffer";

    websocket.uid = socketID;
    WebSocketArray[socketID] = websocket;

    websocket.onopen = function(evt) {
      Module['dynCall_vi'](WebSocketCallbacks[socketID].open, socketID);
      //Runtime.dynCall('v', ConnectionOpen.callback, 0);
    };

    websocket.onclose = function(evt) {
      Module['dynCall_vi'](WebSocketCallbacks[socketID].closed, socketID);
      //Runtime.dynCall('v', ConnectionClosed.callback, 0);
    };

    websocket.onmessage = function(evt) {
      
      if (evt.data instanceof ArrayBuffer) {
        console.log("onmessage ArrayBuffer");
        var byteArray = new Uint8Array(evt.data);
        var buffer = _malloc(byteArray.byteLength);

        HEAPU8.set(byteArray, buffer / byteArray.BYTES_PER_ELEMENT);
        Module['dynCall_viii'](WebSocketCallbacks[socketID].onByteArrayMessage, socketID, buffer, byteArray.length);
        //Runtime.dynCall('vii', ReceivedByteArrayMessage.callback, [buffer, byteArray.length]);
        _free(buffer);
      } else if (typeof evt.data === "string") {
        console.log("onmessage string");
        var buffer = _malloc(lengthBytesUTF8(evt.data) + 1);

        stringToUTF8(evt.data, buffer, lengthBytesUTF8(evt.data) + 1);
	      Module['dynCall_vii'](WebSocketCallbacks[socketID].onTextMessage, socketID, buffer);
        //Runtime.dynCall('vi', ReceivedTextMessage.callback, [buffer]);
        _free(buffer);
      }
    };

    websocket.onerror = function(evt) {
      Module['dynCall_vi'](WebSocketCallbacks[socketID].error, socketID);
      //Runtime.dynCall('v', ReceivedError.callback, 0);
    };
  },

  DisconnectWebSocket: function (socketID) {
    WebSocketArray[socketID].close();
  },

//this should be called before other callback setup functions
  SetupConnectionOpenCallbackFunction: function (sid, obj) {
    WebSocketCallbacks[sid] = {};
    WebSocketCallbacks[sid].open = obj;
  },

  SetupConnectionClosedCallbackFunction: function (sid, obj) {
    WebSocketCallbacks[sid].closed = obj;
  },

  SetupReceivedByteArrayMessageCallbackFunction: function (sid, obj) {
    WebSocketCallbacks[sid].onByteArrayMessage = obj;
  },

  SetupReceivedTextMessageCallbackFunction: function (sid, obj) {
    WebSocketCallbacks[sid].onTextMessage = obj;
  },

  SetupReceivedErrorCallbackFunction: function (sid, obj) {
    WebSocketCallbacks[sid].error = obj;
  },

  SendTextMessage: function (sid, textMessage) {
    var message = Pointer_stringify(textMessage);
    WebSocketArray[sid].send(message);
  },

  SendByteArrayMessage: function (sid, array, size) {
    var byteArray = new Uint8Array(size);

    for(var i = 0; i < size; i++) {
      var byte = HEAPU8[array + i];
      byteArray[i] = byte;
    }

    WebSocketArray[sid].send(byteArray.buffer);
  },
};

autoAddDeps(WebSocketJavaScriptLibrary, '$WebSocketArray');
autoAddDeps(WebSocketJavaScriptLibrary, '$WebSocketCallbacks');
mergeInto(LibraryManager.library, WebSocketJavaScriptLibrary);
