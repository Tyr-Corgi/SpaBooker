// SpaBooker JavaScript helper functions

/**
 * Download a file from base64 data
 * @param {string} filename - The name of the file to download
 * @param {string} contentType - The MIME type of the file
 * @param {string} base64Data - The base64 encoded file data
 */
function downloadFile(filename, contentType, base64Data) {
    const linkElement = document.createElement('a');
    linkElement.setAttribute('href', `data:${contentType};base64,${base64Data}`);
    linkElement.setAttribute('download', filename);
    linkElement.style.display = 'none';
    
    document.body.appendChild(linkElement);
    linkElement.click();
    document.body.removeChild(linkElement);
}

/**
 * Copy text to clipboard
 * @param {string} text - The text to copy
 * @returns {Promise<boolean>} - True if successful, false otherwise
 */
async function copyToClipboard(text) {
    try {
        await navigator.clipboard.writeText(text);
        return true;
    } catch (err) {
        console.error('Failed to copy text: ', err);
        return false;
    }
}

/**
 * Show a browser notification (if permission granted)
 * @param {string} title - The notification title
 * @param {string} body - The notification body
 * @param {string} icon - Optional icon URL
 */
async function showNotification(title, body, icon = '/favicon.png') {
    if ('Notification' in window && Notification.permission === 'granted') {
        new Notification(title, { body, icon });
    }
}

/**
 * Request notification permission
 * @returns {Promise<string>} - The permission result
 */
async function requestNotificationPermission() {
    if ('Notification' in window) {
        return await Notification.requestPermission();
    }
    return 'denied';
}

