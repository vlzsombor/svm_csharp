// Canvas setup
var canvas = new fabric.Canvas('canvas');
canvas.isDrawingMode = true;
canvas.freeDrawingBrush.width = 1;
canvas.freeDrawingBrush.color = "#000000";
canvas.backgroundColor = "#ffffff";
canvas.renderAll();

// Clear button callback
$("#clear-canvas").click(function(){ 
  canvas.clear(); 
  canvas.backgroundColor = "#ffffff";
  canvas.renderAll();
  updateChart(zeros);
  $("#status").removeClass();
});


// Predict button callback
$("#predict").click(function(){  
  // Change status indicator
  $("#status").removeClass().toggleClass("fa fa-spinner fa-spin");

  // Get canvas contents as url
  var fac = (1.) / 13.; 
  var url = canvas.toDataURLWithMultiplier('png', fac);
  
  console.log(url)
  // Post url to python script
  var jq = $.post('cgi-bin/mnist.py', url)
    .done(function (json) {
      if (json.result) {
        $("#status").removeClass().toggleClass("fa fa-check");
        $('#svg-chart').show();
        updateChart(json.data);
      } else {
         $("#status").removeClass().toggleClass("fa fa-exclamation-triangle");
         console.log('Script Error: ' + json.error)
      }
    })
    .fail(function (xhr, textStatus, error) {
      $("#status").removeClass().toggleClass("fa fa-exclamation-triangle");
      console.log("POST Error: " + xhr.responseText + ", " + textStatus + ", " + error);
    }
  );

});



async function sendRequest() {
    var twod = canvas.getContext("2d")
    const imageData = twod.getImageData(0, 0, canvas.width, canvas.height);
//    const input = document.getElementById('inputText').value;
    let csv = '';
    
    const data = imageData.data

    for(var i = 0; i < data.length; i+=4) {
        const r = data[i];
        const g = data[i+1];
        const b = data[i+2];
        const a = data[i+3];
        const gray = Math.round(0.299 * r + 0.587 * g + 0.114 * b);
        csv+= `${gray},`
    }

    
    const response = await fetch('/svm', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(imageData.data.toString())
    });
    
    
    
    
    
    
    
    
    const char = await response.text();
    document.getElementById('result').textContent = char;
}
