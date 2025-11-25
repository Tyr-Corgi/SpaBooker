window.schedulerHelpers = {
    confirmUnsavedChanges: function () {
        return confirm("You have unsaved changes. Are you sure you want to close without saving?");
    }
};

// Download file helper function
window.downloadFile = function (filename, content, contentType) {
    const blob = new Blob([content], { type: contentType });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
};

