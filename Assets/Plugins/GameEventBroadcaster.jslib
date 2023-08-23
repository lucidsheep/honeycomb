mergeInto(LibraryManager.library, {
      // Method used to send a message to the page
   JSGameEvent: function (text) {
      // Convert bytes to the text
      var convertedText = Pointer_stringify(text);
      // Pass message to the page
      onGameEvent(convertedText); // This function is embeded into the page
   }
});