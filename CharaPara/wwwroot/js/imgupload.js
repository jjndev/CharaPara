//alert("imgupload.js running");

$(document).ready(function () {
    //alert("imgupload.js ready");

    var form = $('#imgupload-dataForm');
    var submitButton = $('#imgupload-submitButton');
    submitButton.prop('disabled', true);

    if (form.valid()) {
        submitButton.prop('disabled', false);
    } else {
        submitButton.prop('disabled', true);
    }

    form.on('change keyup', function () {
        if (form.valid()) {
            submitButton.prop('disabled', false);
        } else {
            submitButton.prop('disabled', true);
        }
    });

    form.validate();

    document.getElementById('imgupload-submitButton').onclick = function () {

        if (submitButton.prop('disabled')) {
            form.validate();
            return;
        }

        var fileForm = document.getElementById('imgupload-fileForm');
        var fileInput = fileForm.querySelector('input[type="file"]');
        //var fileInput = document.getElementById('fileUpload');
        var file = fileInput.files[0];
        //var preSignedUrl = fileForm.querySelector('input[type="hidden"]').value;
        //var preSignedUrl = uploadPreSignedUrl;

        alert(preSignedUrl);

        if (!file) { return; }

        var fileType = encodeURIComponent(file.type);
        var xhr = new XMLHttpRequest();

        xhr.open("PUT", preSignedUrl, true);
        xhr.setRequestHeader('Content-Type', fileType);

        xhr.onload = function () {
            if (xhr.status === 200) {
                document.getElementById('imgupload-dataForm').submit(); // Submit the form

            } else {
                console.error("An error occurred during the image upload:", xhr.statusText);
                console.error(xhr.responseXML);
                // Handle different status codes here
                switch (xhr.status) {
                    case 400:
                        alert("(400) Bad Request: The server could not understand the request due to invalid syntax.");
                        break;
                    case 403:
                        alert("(403) Forbidden: You might not have the necessary permissions for the resource.");
                        break;
                    case 404:
                        alert("(404) Not Found: The requested resource was not found.");
                        break;
                    case 500:
                        alert("(500) Internal Server Error: The server has encountered a situation it doesn't know how to handle.");
                        break;
                    default:
                        alert(`(${xhr.status}) Image upload failed. `);
                }
            }
        };

        xhr.onerror = function () {
            // A more specific error can be logged to the console
            console.error("An error occurred during the image upload:", xhr.statusText);
            alert("An error occurred during the image upload.");
        };

    xhr.send(file); // Send the file
    };

});












/*

$(document).ready(function () {
    document.getElementById('imgupload-submitButton').onclick = function () {
        event.preventDefault();

        // Retrieve the selected file from the form
        var fileForm = document.getElementById('imgupload-fileForm');
        var fileInput = fileForm.querySelector('input[type="file"]');


        let file = fileInput.files[0];

        if (!file) {
            //alert("No file selected");
            return;
        }
        let preSignedUrl = fileForm.querySelector('input[type="hidden"]').value;

        // Fetch the pre-signed URL from your server
        // Upload the file to S3 using the pre-signed URL
        fetch(preSignedUrl, {
            method: 'PUT',
            headers: {
                'Content-Type': file.type  // Ensure the Content-Type matches the type expected by the pre-signed URL
            },
            body: file  // The actual file to be uploaded
        })
            .then(response => {
                if (response.ok) {
                    console.log('File successfully uploaded to S3');

                    document.getElementById('imgupload-dataForm').submit(); // Submit the form

                } else {
                    console.error('Upload failed:', response);
                }
            })
            .catch(error => console.error('Error uploading to S3:', error));
    };
});

*/