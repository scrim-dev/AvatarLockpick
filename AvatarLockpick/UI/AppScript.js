/**
 * AvatarLockpick GUI - Consolidated JavaScript
 */

// --- Elements Cache ---
let splashScreen, appWrapper, tabButtons, tabPanels, animationsToggle, userIdInput, censorUserIdToggle;
let avatarIdInput, responseSpan, colorPickers, htmlElement, actionPopup, popupTitle, popupMessage;
let unlockNoticePopup, noticeTimerSpan, sidebarToggle, loadedAvatarInfoPanel, loadedAvatarIdSpan;
let loadedUserIdSpan, unlockActionButtons, unlockWarningPanel, confirmUnloadPopup, presetButtons;
let historyListContainer, noHistoryMessage, importJsonPopup, importJsonTextarea, importErrorMessage;
let netResponsePopup, netResponseContent, notePopup, noteTextarea, appLogsContainer, backendMessagesToggle;

// --- Theme Objects ---
const themes = {
    dodgerblue: {
        '--bg-color': '#121212',
        '--bg-secondary-color': '#181818',
        '--text-color': '#ffffff',
        '--accent-color': '#1E90FF',
        '--accent-hover-color': '#46A3FF',
        '--panel-bg-color': '#242424',
        '--border-color': '#333333',
        '--input-bg-color': '#2d2d2d',
        '--input-border-color': '#454545'
    },
    crimson: {
        '--accent-color': '#DC143C',
        '--accent-hover-color': '#E6395A',
    },
    green: {
        '--accent-color': '#2ECC71',
        '--accent-hover-color': '#58D68D',
    },
    magenta: {
        '--accent-color': '#DE3BFF',
        '--accent-hover-color': '#E566FF',
    }
};

const defaultTheme = themes.dodgerblue;

// Global state variables
let isAvatarLoaded = false;
let loadedAvatarId = null;
let loadedUserId = null;
let avatarHistory = [];
let netLog = ["Waiting for messages from .NET..."];
let currentEditingAvatarId = null;
let noticeShownThisSession = false;
let noticeTimerInterval = null;
let showBackendMessages = false; // Toggle for showing backend communication popups

// --- Photino Communication Wrapper ---
function sendMessageToDotNet(messageObject) {
    if (window.external && typeof window.external.sendMessage === 'function') {
        const messageString = JSON.stringify(messageObject);
        console.log("Sending to .NET:", messageString);
        window.external.sendMessage(messageString);
    } else {
        console.error("Photino communication (window.external.sendMessage) not available.");
    }
}

// --- Helper Functions ---
function applyTheme(themeSettings, saveToLocalStorage = true) {
    Object.keys(themeSettings).forEach(key => {
        if (themeSettings.hasOwnProperty(key)) {
            htmlElement.style.setProperty(key, themeSettings[key]);
        }
    });
    if (themeSettings.hasOwnProperty('--accent-color')){
        updateAccentRgbVar(themeSettings['--accent-color']);
    }
    updateColorPickers();

    if (saveToLocalStorage) {
        let matchedPreset = null;
        for (const name in themes) {
            const presetFull = { ...defaultTheme, ...themes[name] };
            let isMatch = true;
            Object.keys(presetFull).forEach(key => {
                if (themeSettings[key] !== presetFull[key]) {
                    isMatch = false;
                }
            });
            if (isMatch && Object.keys(themeSettings).length === Object.keys(presetFull).length) {
                matchedPreset = name;
                break;
            }
        }

        if (matchedPreset) {
            console.log(`Saving applied theme as preset: ${matchedPreset}`);
            localStorage.setItem('themeType', 'preset');
            localStorage.setItem('themeName', matchedPreset);
            colorPickers.forEach(picker => {
                const cssVar = picker.getAttribute('data-var');
                localStorage.removeItem(`customColor_${cssVar}`);
            });
        } else {
            console.log("Saving applied theme as custom.");
            localStorage.setItem('themeType', 'custom');
            localStorage.removeItem('themeName');
            Object.keys(themeSettings).forEach(key => {
                if (key.startsWith('--')) {
                    localStorage.setItem(`customColor_${key}`, themeSettings[key]);
                }
            });
        }
    }
}

function updateColorPickers() {
    colorPickers.forEach(picker => {
        const cssVar = picker.getAttribute('data-var');
        picker.value = getComputedStyle(htmlElement).getPropertyValue(cssVar).trim();
    });
}

function hexToRgb(hex) {
    let result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
    return result ? {
        r: parseInt(result[1], 16),
        g: parseInt(result[2], 16),
        b: parseInt(result[3], 16)
    } : null;
}

function updateAccentRgbVar(hexColor) {
    const rgb = hexToRgb(hexColor);
    if (rgb) {
        htmlElement.style.setProperty('--accent-color-rgb', `${rgb.r},${rgb.g},${rgb.b}`);
    }
}

// --- Initialization ---
function initialize() {
    const savedThemeType = localStorage.getItem('themeType');
    const savedThemeName = localStorage.getItem('themeName');
    let themeToApply = { ...defaultTheme };

    if (savedThemeType === 'preset' && savedThemeName && themes[savedThemeName]) {
        console.log(`Initializing with saved preset theme: ${savedThemeName}`);
        themeToApply = { ...defaultTheme, ...themes[savedThemeName] };
    } else if (savedThemeType === 'custom') {
        console.log("Initializing with saved custom theme settings.");
        colorPickers.forEach(picker => {
            const cssVar = picker.getAttribute('data-var');
            const savedColor = localStorage.getItem(`customColor_${cssVar}`);
            if (savedColor) {
                themeToApply[cssVar] = savedColor;
            }
        });
    } else {
        console.log("Initializing with default theme (Dodger Blue).");
    }
    applyTheme(themeToApply, false);

    const sidebarExpandedStr = localStorage.getItem('sidebarExpanded');
    const sidebarExpanded = sidebarExpandedStr !== 'false';
    appWrapper.classList.toggle('sidebar-expanded', sidebarExpanded);
    updateSidebarToggleIcon(sidebarExpanded);

    const savedAnimationPref = localStorage.getItem('animationsEnabled');
    if (savedAnimationPref !== null) {
        animationsToggle.checked = JSON.parse(savedAnimationPref);
    }
    document.body.classList.toggle('no-animations', !animationsToggle.checked);

    const savedCensorPref = localStorage.getItem('censorUserId');
    if (savedCensorPref !== null) {
        censorUserIdToggle.checked = JSON.parse(savedCensorPref);
        userIdInput.type = censorUserIdToggle.checked ? 'password' : 'text';
    }

    const noticeAlreadyShown = localStorage.getItem('unlockNoticeShown') === 'true';
    if (noticeAlreadyShown) {
        noticeShownThisSession = true;
        console.log("Unlock notice has been shown in a previous session.");
    }

    const savedAvatarId = localStorage.getItem('avatarId');
    const savedUserId = localStorage.getItem('userId');
    if (savedAvatarId) {
        avatarIdInput.value = savedAvatarId;
    }
    if (savedUserId) {
        userIdInput.value = savedUserId;
    }

    initLinuxSettings();

    const savedHistory = localStorage.getItem('avatarHistory');
    if (savedHistory) {
        try {
            avatarHistory = JSON.parse(savedHistory);
        } catch (e) {
            console.error("Error parsing avatar history from localStorage:", e);
            avatarHistory = [];
        }
    }
    renderHistory();

    updateUnlockUI();
}

function updateSidebarToggleIcon(isExpanded) {
    const icon = sidebarToggle.querySelector('i');
    if (icon) {
        icon.className = isExpanded ? 'fa-solid fa-arrow-left' : 'fa-solid fa-bars';
    }
}

// --- Button Click Functions ---
function callDotNetSendIDs() {
    const avatarId = avatarIdInput.value;
    const userId = userIdInput.value;

    const message = { type: 'avatarInfo', avatarId, userId };
    sendMessageToDotNet(message);
}

function loadAvatarInfo() {
    const avatarId = avatarIdInput.value;
    const userId = userIdInput.value;

    if (!avatarId) {
        console.error("Avatar ID is required.");
        alert("Please enter an Avatar ID.");
        return;
    }

    console.log(`Loading info for Avatar: ${avatarId}, User: ${userId || 'N/A'}`);

    loadedAvatarIdSpan.textContent = avatarId;
    loadedUserIdSpan.textContent = censorUserIdToggle.checked ? '********' : (userId || 'N/A');

    isAvatarLoaded = true;
    window.loadedAvatarId = avatarId;
    window.loadedUserId = userId;

    updateUnlockUI();
    addAvatarToHistory(avatarId, userId);
}

function triggerPopup(actionType) {
    if (!isAvatarLoaded || !window.loadedAvatarId) {
        console.error("No avatar loaded. Cannot perform unlock action.");
        showNoAvatarWarning();
        return;
    }

    console.log(`${actionType} button clicked for Avatar: ${window.loadedAvatarId}`);

    // Only show popup if debug/verbose mode is enabled
    if (showBackendMessages) {
        popupTitle.textContent = `${actionType} Action`;
        popupMessage.textContent = `Calling .NET...`;
        actionPopup.classList.add('show');
    }

    const message = {
        type: 'unlockAction',
        action: actionType,
        avatarId: window.loadedAvatarId,
        userId: window.loadedUserId || null
    };
    sendMessageToDotNet(message);

    // Auto-switch to Logs tab
    const logsTabBtn = document.querySelector('.tab-button[data-tab="logs"]');
    if (logsTabBtn) {
        logsTabBtn.click();
    }
}

function openHelpUrl() {
    console.log("Requesting help URL from .NET...");
    const message = { type: 'openHelpUrl' };
    sendMessageToDotNet(message);
}

function triggerRestart(restartType) {
    if (!isAvatarLoaded || !window.loadedAvatarId) {
        console.error("No avatar loaded. Restart action might depend on loaded avatar.");
    }

    console.log(`Triggering restart: ${restartType}`);

    const message = { type: restartType };
    sendMessageToDotNet(message);

    // Only show popup if debug/verbose mode is enabled
    if (showBackendMessages) {
        popupTitle.textContent = restartType === 'restart-vr' ? 'Restart (VR)' : 'Restart';
        popupMessage.textContent = `Requesting ${popupTitle.textContent}...`;
        actionPopup.classList.add('show');
    }
}

// --- Custom / Linux Settings Functions ---
function initLinuxSettings() {
    const savedLinuxPath = localStorage.getItem('customLinuxPath');
    if (savedLinuxPath && document.getElementById('custom-linux-cache-path')) {
        document.getElementById('custom-linux-cache-path').value = savedLinuxPath;
        sendLinuxSettingsToBackend(savedLinuxPath);
    }
}

function saveLinuxSettings() {
    const linuxPath = document.getElementById('custom-linux-cache-path').value.trim();
    localStorage.setItem('customLinuxPath', linuxPath);
    sendLinuxSettingsToBackend(linuxPath);
    
    popupTitle.textContent = "Linux Settings Saved";
    popupMessage.textContent = "Custom cache path has been updated.";
    actionPopup.classList.add('show');
}

function resetLinuxSettings() {
    document.getElementById('custom-linux-cache-path').value = '';
    localStorage.removeItem('customLinuxPath');
    sendLinuxSettingsToBackend("");
    
    popupTitle.textContent = "Linux Settings Reset";
    popupMessage.textContent = "Now using default Proton cache path.";
    actionPopup.classList.add('show');
}

function sendLinuxSettingsToBackend(linuxPath) {
    const message = {
        type: 'linuxSettings',
        path: linuxPath
    };
    sendMessageToDotNet(message);
}

function resetTheme() {
    console.log("Resetting to default theme (Dodger Blue)");
    localStorage.removeItem('themeType');
    localStorage.removeItem('themeName');
    colorPickers.forEach(picker => {
        const cssVar = picker.getAttribute('data-var');
        localStorage.removeItem(`customColor_${cssVar}`);
    });
    applyTheme(themes.dodgerblue);
}

// --- History / Recents Tab Functions ---
function renderHistory() {
    historyListContainer.innerHTML = '';
    if (avatarHistory.length === 0) {
        if (noHistoryMessage) noHistoryMessage.style.display = 'block';
    } else {
        if (noHistoryMessage) noHistoryMessage.style.display = 'none';
        avatarHistory.forEach(item => {
            const itemDiv = document.createElement('div');
            itemDiv.className = 'history-item';
            const formattedDate = new Date(item.timestamp).toLocaleString();
            const noteContent = item.note 
                ? `<p class="history-item-note">${item.note}</p>`
                : `<p class="history-item-note placeholder">Looks like this avatar is a mystery. Add a note so your future self knows what's up!</p>`;

            itemDiv.innerHTML = `
                <h4>${item.avatarId}</h4>
                <p><strong>User:</strong> ${item.userId || 'N/A'}</p>
                <p><small>Loaded: ${formattedDate}</small></p>
                <div class="history-item-note-container">
                    ${noteContent}
                </div>
                <div class="history-item-actions">
                    <button class="action-button" onclick="loadFromHistory('${item.avatarId}', '${item.userId}')"><i class="fa-solid fa-upload"></i> Load</button>
                    <button class="action-button secondary-action" onclick="showNotePopup('${item.avatarId}')"><i class="fa-solid fa-pencil"></i> ${item.note ? 'Edit' : 'Add'} Note</button>
                    <button class="action-button secondary-action" onclick="deleteFromHistory(event, '${item.avatarId}')"><i class="fa-solid fa-trash"></i> Delete</button>
                </div>
            `;
            historyListContainer.appendChild(itemDiv);
        });
    }
}

function addAvatarToHistory(avatarId, userId) {
    let existingNote = null;
    const existingIndex = avatarHistory.findIndex(item => item.avatarId === avatarId);
    if (existingIndex > -1) {
        existingNote = avatarHistory[existingIndex].note;
        avatarHistory.splice(existingIndex, 1);
    }

    avatarHistory.unshift({
        avatarId: avatarId,
        userId: userId,
        timestamp: new Date().getTime(),
        note: existingNote
    });

    if (avatarHistory.length > 50) {
        avatarHistory.pop();
    }

    localStorage.setItem('avatarHistory', JSON.stringify(avatarHistory));
    renderHistory();
}

function loadFromHistory(avatarId, userId) {
    avatarIdInput.value = avatarId;
    userIdInput.value = userId;
    
    localStorage.setItem('avatarId', avatarId);
    localStorage.setItem('userId', userId);

    document.querySelector('.tab-button[data-tab="avatar"]').click();
    loadAvatarInfo();
}

function showNotePopup(avatarId) {
    currentEditingAvatarId = avatarId;
    const historyItem = avatarHistory.find(item => item.avatarId === avatarId);
    if (historyItem) {
        noteTextarea.value = historyItem.note || '';
        notePopup.classList.add('show');
    }
}

function closeNotePopup() {
    notePopup.classList.remove('show');
    currentEditingAvatarId = null;
}

function saveNote() {
    if (currentEditingAvatarId) {
        const historyItem = avatarHistory.find(item => item.avatarId === currentEditingAvatarId);
        if (historyItem) {
            historyItem.note = noteTextarea.value.trim() ? noteTextarea.value : null;
            localStorage.setItem('avatarHistory', JSON.stringify(avatarHistory));
            renderHistory();
        }
    }
    closeNotePopup();
}

function deleteFromHistory(event, avatarId) {
    event.stopPropagation();
    avatarHistory = avatarHistory.filter(item => item.avatarId !== avatarId);
    localStorage.setItem('avatarHistory', JSON.stringify(avatarHistory));
    renderHistory();
}

function exportHistory() {
    if (avatarHistory.length === 0) {
        popupTitle.textContent = "Export Failed";
        popupMessage.textContent = "History is empty. Nothing to export.";
        actionPopup.classList.add('show');
        return;
    }
    const jsonString = JSON.stringify(avatarHistory, null, 2);
    const message = { type: 'exportHistory', payload: jsonString };
    sendMessageToDotNet(message);

    popupTitle.textContent = "Export Successful";
    popupMessage.textContent = "History has been sent to the application for exporting.";
    actionPopup.classList.add('show');
}

function showImportPopup() {
    importJsonTextarea.value = '';
    importErrorMessage.style.display = 'none';
    importJsonPopup.classList.add('show');
}

function closeImportPopup() {
    importJsonPopup.classList.remove('show');
}

function importHistory() {
    const jsonString = importJsonTextarea.value;
    if (!jsonString) {
        importErrorMessage.textContent = "Textarea is empty.";
        importErrorMessage.style.display = 'block';
        return;
    }

    try {
        const importedData = JSON.parse(jsonString);
        if (!Array.isArray(importedData) || !importedData.every(item => item.avatarId && item.timestamp)) {
            throw new Error("Invalid history format. Must be an array of objects with 'avatarId' and 'timestamp'.");
        }
        avatarHistory = importedData.map(item => ({
            avatarId: item.avatarId,
            userId: item.userId || null,
            timestamp: item.timestamp,
            note: item.note || null
        }));
        localStorage.setItem('avatarHistory', JSON.stringify(avatarHistory));
        renderHistory();
        closeImportPopup();
    } catch (e) {
        importErrorMessage.textContent = `Import failed: ${e.message}`;
        importErrorMessage.style.display = 'block';
    }
}

function closePopup() {
    if (actionPopup) {
        actionPopup.classList.remove('show');
    }
}

// --- Logs Tab Functions ---
function appendAppLog(logData) {
    const firstEntry = appLogsContainer.querySelector('.log-entry');
    if (firstEntry && firstEntry.textContent.includes("Waiting for logs...")) {
        appLogsContainer.innerHTML = '';
    }

    const entryDiv = document.createElement('div');
    entryDiv.className = 'log-entry';

    let color = 'var(--text-color)';
    let icon = '';
    
    switch (logData.logType) {
        case 'success':
            color = '#00ff40';
            icon = '<i class="fa-solid fa-check-circle"></i>';
            break;
        case 'warn':
            color = '#ffcc00';
            icon = '<i class="fa-solid fa-triangle-exclamation"></i>';
            break;
        case 'error':
            color = '#ff002b';
            icon = '<i class="fa-solid fa-circle-xmark"></i>';
            break;
        case 'info':
        default:
            color = 'var(--text-color)';
            icon = '<i class="fa-solid fa-info-circle"></i>';
            break;
    }

    entryDiv.innerHTML = `
        <span style="color: #00ff5e; font-size: 0.9em;">[${logData.timestamp}]</span>
        <span style="color: #0084ff; font-weight: bold;">[${logData.task}]</span>
        <span style="color: ${color};">${icon} ${logData.message}</span>
    `;

    appLogsContainer.appendChild(entryDiv);
    appLogsContainer.scrollTop = appLogsContainer.scrollHeight;
}

function clearAppLogs() {
    appLogsContainer.innerHTML = '<div class="log-entry"><span class="log-system">[System]</span> Logs cleared.</div>';
}

function copyAppLogs() {
    const text = appLogsContainer.innerText;
    navigator.clipboard.writeText(text).then(() => {
        const btn = document.querySelector('button[onclick="copyAppLogs()"]');
        const originalHTML = btn.innerHTML;
        btn.innerHTML = '<i class="fa-solid fa-check"></i> Copied!';
        setTimeout(() => { btn.innerHTML = originalHTML; }, 2000);
    }).catch(err => {
        console.error('Failed to copy logs: ', err);
    });
}

// --- .NET Response Popup Functions ---
function showNetResponsePopup() {
    if (netResponsePopup) {
        netResponseContent.textContent = netLog.join('\n');
        netResponsePopup.classList.add('show');
    }
}

function closeNetResponsePopup() {
    if (netResponsePopup) {
        netResponsePopup.classList.remove('show');
    }
}

function clearNetLog() {
    netLog = ["Log cleared."];
    if (netResponseContent) {
        netResponseContent.textContent = netLog[0];
    }
}

// --- Unlock UI Functions ---
function updateUnlockUI() {
    if (isAvatarLoaded) {
        loadedAvatarInfoPanel.style.display = 'block';
        unlockWarningPanel.style.display = 'none';
        unlockActionButtons.forEach(btn => btn.disabled = false);
    } else {
        loadedAvatarInfoPanel.style.display = 'none';
        unlockWarningPanel.style.display = 'block';
        unlockActionButtons.forEach(btn => btn.disabled = true);
    }
}

function showUnloadConfirmation() {
    if (confirmUnloadPopup) {
        confirmUnloadPopup.classList.add('show');
    }
}

function confirmUnloadAvatar() {
    console.log("Unloading avatar...");
    isAvatarLoaded = false;
    loadedAvatarId = null;
    loadedUserId = null;
    updateUnlockUI();
    cancelUnloadAvatar();
}

function cancelUnloadAvatar() {
    if (confirmUnloadPopup) {
        confirmUnloadPopup.classList.remove('show');
    }
}

// --- No Avatar Warning Popup ---
function showNoAvatarWarning() {
    popupTitle.textContent = "No Avatar Loaded";
    popupMessage.innerHTML = '<i class="fa-solid fa-triangle-exclamation" style="color: #ffcc00; margin-right: 8px;"></i> Please load an avatar from the <strong>Avatar</strong> tab first before performing unlock actions.';
    actionPopup.classList.add('show');
}

// --- Unlock Notice Popup Logic ---
function closeUnlockNotice() {
    if (unlockNoticePopup) {
        unlockNoticePopup.classList.remove('show');
        if (noticeTimerInterval) {
            clearInterval(noticeTimerInterval);
            noticeTimerInterval = null;
            console.log("Unlock notice popup closed manually.");
        }
    }
}

function showUnlockNotice() {
    if (unlockNoticePopup && !noticeShownThisSession) {
        noticeShownThisSession = true;
        localStorage.setItem('unlockNoticeShown', 'true');
        console.log("Showing unlock notice popup.");

        let secondsLeft = 20;
        noticeTimerSpan.textContent = secondsLeft;
        unlockNoticePopup.classList.add('show');

        if (noticeTimerInterval) {
            clearInterval(noticeTimerInterval);
        }

        noticeTimerInterval = setInterval(() => {
            secondsLeft--;
            if (noticeTimerSpan) {
                noticeTimerSpan.textContent = secondsLeft;
            }
            if (secondsLeft <= 0) {
                closeUnlockNotice();
            }
        }, 1000);
    }
}

// --- Clear Cache & Reload ---       
function clearCacheAndReload() {
    const confirmation = confirm("Are you sure you want to clear locally stored settings (theme, sidebar state, etc.) and reload the GUI? This action cannot be undone.");
    if (confirmation) {
        console.log("Clearing local storage and reloading...");
        try {
            localStorage.removeItem('sidebarExpanded');
            localStorage.removeItem('animationsEnabled');
            localStorage.removeItem('censorUserId');
            localStorage.removeItem('themeType');
            localStorage.removeItem('themeName');
            localStorage.removeItem('unlockNoticeShown');
            localStorage.removeItem('avatarHistory');
            colorPickers.forEach(picker => {
                const cssVar = picker.getAttribute('data-var');
                localStorage.removeItem(`customColor_${cssVar}`);
            });
            
            window.location.reload();
        } catch (error) {
            console.error("Error clearing local storage or reloading:", error);
            alert("An error occurred while trying to clear cache and reload.");
        }
    } else {
        console.log("Clear cache and reload cancelled.");
    }
}

// --- Clear Input Fields Logic ---
function clearInputFields() {
    console.log("Clearing Avatar and User ID fields and cache.");
    avatarIdInput.value = '';
    userIdInput.value = '';
    localStorage.removeItem('avatarId');
    localStorage.removeItem('userId');
}

// --- DOM Ready & Event Listeners ---
document.addEventListener('DOMContentLoaded', function() {
    // Cache all DOM elements
    splashScreen = document.getElementById('splash-screen');
    appWrapper = document.getElementById('app-wrapper');
    tabButtons = document.querySelectorAll('.tab-button');
    tabPanels = document.querySelectorAll('.tab-panel');
    animationsToggle = document.getElementById('animations-toggle');
    userIdInput = document.getElementById('user-id');
    censorUserIdToggle = document.getElementById('censor-user-id');
    avatarIdInput = document.getElementById('avatar-id');
    responseSpan = document.getElementById('dotnet-response');
    colorPickers = document.querySelectorAll('input[type="color"]');
    htmlElement = document.documentElement;
    actionPopup = document.getElementById('action-popup');
    popupTitle = document.getElementById('popup-title');
    popupMessage = document.getElementById('popup-message');
    unlockNoticePopup = document.getElementById('unlock-notice-popup');
    noticeTimerSpan = document.getElementById('notice-timer');
    sidebarToggle = document.getElementById('sidebar-toggle');
    loadedAvatarInfoPanel = document.getElementById('loaded-avatar-info-panel');
    loadedAvatarIdSpan = document.getElementById('loaded-avatar-id');
    loadedUserIdSpan = document.getElementById('loaded-user-id');
    unlockActionButtons = document.querySelectorAll('.unlock-action-btn');
    unlockWarningPanel = document.getElementById('unlock-warning-panel');
    confirmUnloadPopup = document.getElementById('confirm-unload-popup');
    presetButtons = document.querySelectorAll('.preset-btn');
    historyListContainer = document.getElementById('history-list-container');
    noHistoryMessage = document.getElementById('no-history-message');
    importJsonPopup = document.getElementById('import-json-popup');
    importJsonTextarea = document.getElementById('import-json-textarea');
    importErrorMessage = document.getElementById('import-error-message');
    netResponsePopup = document.getElementById('net-response-popup');
    netResponseContent = document.getElementById('net-response-content');
    notePopup = document.getElementById('note-popup');
    noteTextarea = document.getElementById('note-textarea');
    appLogsContainer = document.getElementById('app-logs-container');
    backendMessagesToggle = document.getElementById('backend-messages-toggle');
});

// --- Window Load Event ---
window.addEventListener('load', () => {
    initialize();
    initCustomDbSettings();

    const languageSelect = document.getElementById('language-select');
    if (languageSelect) {
        languageSelect.value = localStorage.getItem('appLanguage') || 'en';
        languageSelect.addEventListener('change', (e) => {
            updateLanguage(e.target.value);
        });
        updateLanguage(languageSelect.value);
    }

    setTimeout(() => {
        splashScreen.classList.add('fade-out');
    }, 1500);

    splashScreen.addEventListener('transitionend', () => {
        splashScreen.style.display = 'none';
        appWrapper.style.display = 'flex';
    }, { once: true });

    // --- Sidebar Toggle Logic ---
    sidebarToggle.addEventListener('click', () => {
        const isExpanded = appWrapper.classList.toggle('sidebar-expanded');
        localStorage.setItem('sidebarExpanded', isExpanded);
        updateSidebarToggleIcon(isExpanded);
    });

    // --- Tab Switching Logic ---
    tabButtons.forEach(button => {
        button.addEventListener('click', () => {
            tabButtons.forEach(btn => btn.classList.remove('active'));
            tabPanels.forEach(panel => panel.classList.remove('active'));

            button.classList.add('active');
            const tabId = button.getAttribute('data-tab');
            const targetPanel = document.getElementById(`${tabId}-tab`);
            if (targetPanel) {
                targetPanel.classList.add('active');
            }

            if (tabId === 'unlock' && !noticeShownThisSession) {
                showUnlockNotice();
            }
        });
    });

    // --- Settings Logic ---
    animationsToggle.addEventListener('change', (event) => {
        const enableAnimations = event.target.checked;
        document.body.classList.toggle('no-animations', !enableAnimations);
        localStorage.setItem('animationsEnabled', JSON.stringify(enableAnimations));
        console.log(`Animations enabled: ${enableAnimations}`);
    });

    censorUserIdToggle.addEventListener('change', (event) => {
        const censor = event.target.checked;
        userIdInput.type = censor ? 'password' : 'text';
        localStorage.setItem('censorUserId', JSON.stringify(censor));
        console.log(`Censor User ID: ${censor}`);
    });

    // Backend messages toggle
    const savedBackendMsgPref = localStorage.getItem('showBackendMessages');
    if (savedBackendMsgPref !== null) {
        showBackendMessages = JSON.parse(savedBackendMsgPref);
        backendMessagesToggle.checked = showBackendMessages;
    }
    
    backendMessagesToggle.addEventListener('change', (event) => {
        showBackendMessages = event.target.checked;
        localStorage.setItem('showBackendMessages', JSON.stringify(showBackendMessages));
        console.log(`Show backend messages: ${showBackendMessages}`);
    });

    colorPickers.forEach(picker => {
        picker.addEventListener('input', (event) => {
            const cssVar = event.target.getAttribute('data-var');
            const newColor = event.target.value;
            htmlElement.style.setProperty(cssVar, newColor);
            if (cssVar === '--accent-color') {
                updateAccentRgbVar(newColor);
            }
            localStorage.setItem('themeType', 'custom');
            localStorage.setItem(`customColor_${cssVar}`, newColor);
            localStorage.removeItem('themeName');
            console.log(`Saved custom color ${cssVar}: ${newColor}`);
        });
    });

    // Add Preset Button Listeners
    presetButtons.forEach(button => {
        button.addEventListener('click', (event) => {
            const presetName = event.target.getAttribute('data-preset');
            if (themes[presetName]) {
                console.log(`Applying preset theme: ${presetName}`);
                const fullTheme = { ...themes.dodgerblue, ...themes[presetName] };
                applyTheme(fullTheme, true);
                localStorage.setItem('themeType', 'preset');
                localStorage.setItem('themeName', presetName);
                colorPickers.forEach(picker => {
                    const cssVar = picker.getAttribute('data-var');
                    localStorage.removeItem(`customColor_${cssVar}`);
                });
            }
        });
    });

    // --- Disable Context Menu & Dev Tools ---
    document.addEventListener('contextmenu', event => {
        console.log("Context menu blocked.");
        event.preventDefault();
    });

    document.addEventListener('keydown', event => {
        if (event.key === 'F12') {
            console.log("F12 blocked.");
            event.preventDefault();
        }
        if (event.ctrlKey && event.shiftKey && (
            event.key === 'I' || event.key === 'i' ||
            event.key === 'J' || event.key === 'j' ||
            event.key === 'C' || event.key === 'c'
        )) {
            console.log(`Ctrl+Shift+${event.key.toUpperCase()} blocked.`);
            event.preventDefault();
        }
    });

    // --- Input Caching ---
    avatarIdInput.addEventListener('input', (event) => {
        localStorage.setItem('avatarId', event.target.value);
    });

    userIdInput.addEventListener('input', (event) => {
        localStorage.setItem('userId', event.target.value);
    });

    // --- Photino Message Receiver ---
    if (window.external && typeof window.external.receiveMessage === 'function') {
        window.external.receiveMessage(message => {
            // Try parsing as JSON first for logs and progress
            try {
                const data = JSON.parse(message);
                if (data && data.type === 'log') {
                    appendAppLog(data);
                    return;
                }
                if (data && data.type === 'downloadProgress') {
                    updateDownloadProgress(data.progress, data.status, data.title);
                    return;
                }
                if (data && data.type === 'downloadComplete') {
                    closeDownloadProgress();
                    return;
                }
            } catch (e) {
                // Not JSON or failed to parse, treat as plain text
            }

            console.log(`Message received from .NET: ${message}`);

            const timestamp = new Date().toLocaleTimeString();
            const logEntry = `[${timestamp}] ${message}`;
            
            if (netLog.length === 1 && netLog[0] === "Waiting for messages from .NET...") {
                netLog = [logEntry];
            } else {
                netLog.push(logEntry);
            }

            if(netResponseContent) {
                netResponseContent.textContent = netLog.join('\n');
                netResponseContent.scrollTop = netResponseContent.scrollHeight;
            }

            if (message === "ActionCompleteClosePopup") {
                closePopup();
            }
        });
    }
});

// --- Download Progress Popup Functions ---
function showDownloadProgress(title) {
    const popup = document.getElementById('download-progress-popup');
    const titleSpan = document.getElementById('download-title');
    const progressBar = document.getElementById('download-progress-bar');
    const statusText = document.getElementById('download-status');
    
    if (popup) {
        if (titleSpan) titleSpan.textContent = title || 'Downloading...';
        if (progressBar) progressBar.style.width = '0%';
        if (statusText) statusText.textContent = 'Preparing download...';
        popup.classList.add('show');
    }
}

function updateDownloadProgress(progress, status, title) {
    const popup = document.getElementById('download-progress-popup');
    const titleSpan = document.getElementById('download-title');
    const progressBar = document.getElementById('download-progress-bar');
    const statusText = document.getElementById('download-status');
    
    // Show popup if not already visible
    if (popup && !popup.classList.contains('show')) {
        popup.classList.add('show');
    }
    
    if (titleSpan && title) titleSpan.textContent = title;
    if (progressBar) progressBar.style.width = `${progress}%`;
    if (statusText) statusText.textContent = status || `${progress}%`;
}

function closeDownloadProgress() {
    const popup = document.getElementById('download-progress-popup');
    if (popup) {
        popup.classList.remove('show');
    }
}

// --- Custom Database Settings ---
let customDbUrlInput, customDbPathInput;

function initCustomDbSettings() {
    customDbUrlInput = document.getElementById('custom-db-url');
    customDbPathInput = document.getElementById('custom-db-path');
    
    // Load saved settings
    const savedUrl = localStorage.getItem('customDbUrl');
    const savedPath = localStorage.getItem('customDbPath');
    
    if (savedUrl && customDbUrlInput) customDbUrlInput.value = savedUrl;
    if (savedPath && customDbPathInput) customDbPathInput.value = savedPath;
}

function saveCustomDbSettings() {
    const url = customDbUrlInput ? customDbUrlInput.value.trim() : '';
    const path = customDbPathInput ? customDbPathInput.value.trim() : '';
    
    localStorage.setItem('customDbUrl', url);
    localStorage.setItem('customDbPath', path);
    
    // Send to .NET backend
    const message = {
        type: 'customDbSettings',
        url: url,
        path: path
    };
    sendMessageToDotNet(message);
    
    // Show confirmation
    popupTitle.textContent = "Settings Saved";
    popupMessage.innerHTML = '<i class="fa-solid fa-check-circle" style="color: #2ECC71; margin-right: 8px;"></i> Custom database settings have been saved.';
    actionPopup.classList.add('show');
    
    console.log(`Custom DB settings saved - URL: ${url}, Path: ${path}`);
}

function resetCustomDbSettings() {
    if (customDbUrlInput) customDbUrlInput.value = '';
    if (customDbPathInput) customDbPathInput.value = '';
    
    localStorage.removeItem('customDbUrl');
    localStorage.removeItem('customDbPath');
    
    // Notify backend to use default
    const message = {
        type: 'customDbSettings',
        url: '',
        path: ''
    };
    sendMessageToDotNet(message);
    
    popupTitle.textContent = "Settings Reset";
    popupMessage.innerHTML = '<i class="fa-solid fa-rotate-left" style="color: var(--accent-color); margin-right: 8px;"></i> Database settings reset to default.';
    actionPopup.classList.add('show');
    
    console.log("Custom DB settings reset to default");
}// --- Localization Logic ---
const translations = {
    "en": {
        ".tab-button[data-tab='avatar'] .tab-text": "Avatar",
        ".tab-button[data-tab='unlock'] .tab-text": "Unlock",
        ".tab-button[data-tab='recents'] .tab-text": "Recents",
        ".tab-button[data-tab='logs'] .tab-text": "Logs",
        ".tab-button[data-tab='settings'] .tab-text": "Settings",
        ".tab-button[data-tab='help'] .tab-text": "Help",
        ".tab-button[data-tab='info'] .tab-text": "Info",
        "#avatar-tab h2": "<i class=\"fa-solid fa-user\"></i> Avatar",
        "label[for='avatar-id']": "Avatar ID",
        "label[for='user-id']": "User ID",
        "label[for='censor-user-id']": "Censor User ID",
        "button[onclick='callDotNetSendIDs()']": "<i class=\"fa-solid fa-eye\"></i> View Avatar",
        "button[onclick='loadAvatarInfo()']": "<i class=\"fa-solid fa-download\"></i> Load Avatar",
        "button[onclick='clearInputFields()']": "<i class=\"fa-solid fa-eraser\"></i> Clear Fields",
        "#unlock-tab h2": "<i class=\"fa-solid fa-lock-open\"></i> Unlock Actions",
        "#unlock-warning-panel p": "<i class=\"fa-solid fa-triangle-exclamation\"></i> Please load an avatar from the Avatar tab first.",
        "button[onclick=\"triggerPopup('Unlock')\"]": "<i class=\"fa-solid fa-unlock\"></i> Unlock",
        "button[onclick=\"triggerPopup('Unlock')\"] + p": "Will use the general unlock method to unlock avatars.",
        "button[onclick=\"triggerPopup('Unlock (VRCFury)')\"]": "<i class=\"fa-solid fa-bolt\"></i> Unlock (VRCFury)",
        "button[onclick=\"triggerPopup('Unlock (VRCFury)')\"] + p": "Will attempt to unlock VRCFury parameters.",
        "button[onclick=\"triggerPopup('Database Unlock')\"]": "<i class=\"fa-solid fa-database\"></i> Database Unlock",
        "button[onclick=\"triggerPopup('Database Unlock')\"] + p": "Scans avatar's locks against the SQL database of parameters to find a match and unlock.",
        "button[onclick=\"triggerRestart('restart-novr')\"]": "<i class=\"fa-solid fa-arrows-rotate\"></i> Restart",
        "button[onclick=\"triggerRestart('restart-novr')\"] + p": "Restarts VRChat.",
        "button[onclick=\"triggerRestart('restart-vr')\"]": "<i class=\"fa-solid fa-arrows-rotate\"></i> Restart (VR)",
        "button[onclick=\"triggerRestart('restart-vr')\"] + p": "Restarts VRChat in VR mode.",
        "#loaded-avatar-info-panel h3": "Loaded Avatar",
        "#history-h3": "History",
        "#manage-history-h3": "Manage History",
        "#loaded-avi-id-label": "Avatar ID:",
        "#loaded-user-id-label": "User ID:",
        "button[onclick=\"showUnloadConfirmation()\"]": "<i class=\"fa-solid fa-times-circle\"></i> Unload Avatar",
        "#recents-tab h2": "<i class=\"fa-solid fa-history\"></i> Recents",
        "button[onclick='exportHistory()']": "<i class=\"fa-solid fa-file-export\"></i> Export as JSON",
        "button[onclick='showImportPopup()']": "<i class=\"fa-solid fa-file-import\"></i> Import from JSON",
        "#logs-tab h2": "<i class=\"fa-solid fa-file-lines\"></i> Application Logs",
        "button[onclick='clearAppLogs()']": "<i class=\"fa-solid fa-trash\"></i> Clear Logs",
        "button[onclick='copyAppLogs()']": "<i class=\"fa-solid fa-copy\"></i> Copy to Clipboard",
        "#settings-tab h2": "<i class=\"fa-solid fa-gear\"></i> Settings",
        "label[for='animations-toggle']": "Enable UI Animations",
        "label[for='backend-messages-toggle']": "Show Backend Communication Popups",
        "button[onclick='clearCacheAndReload()']": "<i class=\"fa-solid fa-trash-can\"></i> Clear Cache & Reload GUI",
        "#help-tab h2": "<i class=\"fa-solid fa-circle-question\"></i> Help Guide",
        "#help-yt-text": "For an in-depth visual tutorial, be sure to head over to my YouTube channel <strong>@ScrimDev</strong>!",
        "#help-step1-title": "<i class=\"fa-solid fa-clipboard-list\" style=\"margin-right:8px;\"></i> Step 1: Preparation",
        "#help-step1-text": "Launch VRChat and switch into the avatar you wish to unlock. Wait for the avatar to load completely (I recommend waiting a few extra seconds to ensure it is fully cached). Then, enter your VRChat User ID and the Avatar's ID into the application.",
        "#help-step2-title": "<i class=\"fa-solid fa-lock-open\" style=\"margin-right:8px;\"></i> Step 2: Unlocking",
        "#help-step2-text": "Attempt to unlock the avatar using the standard <strong>Unlock</strong> method. If the avatar utilizes custom or VRCFury locks, use <strong>Unlock (VRCFury)</strong> or <strong>Database Unlock</strong> instead.",
        "#help-step3-title": "<i class=\"fa-solid fa-flag-checkered\" style=\"margin-right:8px;\"></i> Finally: Restart",
        "#help-step3-text": "Restart your game using the restart buttons within the app. Enjoy your newly unlocked avatar!",
        "#info-p1": "AvatarLockpick",
        "#info-p2": "GUI made with Photino.NET",
        "#info-p3": "Developed by Scrimmane / ScrimDev",
        "#info-p4": "App Icon by Kmg Design",
        "#info-btn1": "View .NET Log",
        "#info-p5": "<strong>Note:</strong> You can customize this interface! Feel free to edit the files in the application's 'UI' folder to create your own look.",
        "#info-btn2": "Help Page",
        "#settings-theme-h3": "Theme Customization",
        "#settings-presets-h4": "Presets",
        "#settings-preset-default": "Default (Blue)",
        "#settings-custom-colors-h4": "Custom Colors",
        "#settings-bg-label": "Background Color:",
        "#settings-panel-label": "Panel/Secondary BG:",
        "#settings-text-label": "Text Color:",
        "#settings-accent-label": "Accent Color:",
        "#settings-accent-hover-label": "Accent Hover Color:",
        "#settings-reset-theme": "Reset Theme",
        "#settings-prefs-h3": "Preferences",
        "#settings-lang-h3": "Language",
        "#settings-lang-p": "Select your preferred language.",
        "#settings-db-h3": "Custom Database",
        "#settings-db-p": "Load your own lock database from a URL or local file path. Leave empty to use the default database.",
        "#settings-db-url-label": "Database URL",
        "#settings-db-path-label": "Or Local File Path",
        "#settings-db-save": "Save Database Settings",
        "#settings-db-reset": "Reset to Default",
        "#settings-adv-h3": "Advanced",
        "#settings-adv-p1": "Clearing cache will reset theme settings, sidebar state, and User ID censor preference stored in your browser for this GUI.",
        "#settings-adv-btn-clear": "Clear Cache & Reload GUI",
        "#settings-adv-btn-dir": "Open App Directory",
        "#confirm-popup-title": "Unload Avatar?",
        "#confirm-popup-message": "Are you sure you want to unload the current avatar information?",
        "button[onclick='confirmUnloadAvatar()']": "Yes, Unload",
        "button[onclick='cancelUnloadAvatar()']": "Cancel",
        "#note-popup h3": "<i class=\"fa-solid fa-note-sticky\"></i> Edit Note",
        "#note-popup p": "Add or edit your note for the avatar below.",
        "button[onclick='saveNote()']": "Save Note",
        "button[onclick='closeNotePopup()']": "Cancel",
        "#net-response-popup h3": "<i class=\"fa-solid fa-terminal\"></i> .NET Log",
        "button[onclick='closeNetResponsePopup()']": "Close",
        "button[onclick='clearNetLog()']": "Clear Log",
        "#import-json-popup h3": "Import History from JSON",
        "#import-json-popup p": "Paste your JSON content below. This will overwrite your current history.",
        "button[onclick='importHistory()']": "Import",
        "button[onclick='closeImportPopup()']": "Cancel",
        "#unlock-notice-popup h3": "<i class=\"fa-solid fa-info-circle\"></i> Notice",
        "#unlock-notice-popup p": "The 'Unlock using Database' feature scans the avatar file against a database of known lock signatures. This can help unlock avatars even if the locking components are hidden, renamed, or scrambled.",
        "button[onclick='closeUnlockNotice()']": "Close",
        "#download-title": "Downloading Database...",
        "#download-status": "Preparing download...",
        "#no-history-message": "No recent avatars loaded. Load an avatar in the 'Avatar' tab to see it here.",
        "button[onclick='closePopup()']": "Close"
    },
    "es": {
        ".tab-button[data-tab='avatar'] .tab-text": "Avatar",
        ".tab-button[data-tab='unlock'] .tab-text": "Desbloquear",
        ".tab-button[data-tab='recents'] .tab-text": "Recientes",
        ".tab-button[data-tab='logs'] .tab-text": "Registros",
        ".tab-button[data-tab='settings'] .tab-text": "Ajustes",
        ".tab-button[data-tab='help'] .tab-text": "Ayuda",
        ".tab-button[data-tab='info'] .tab-text": "Info",
        "#avatar-tab h2": "<i class=\"fa-solid fa-user\"></i> Avatar",
        "label[for='avatar-id']": "ID de Avatar",
        "label[for='user-id']": "ID de Usuario",
        "label[for='censor-user-id']": "Censurar ID de Usuario",
        "button[onclick='callDotNetSendIDs()']": "<i class=\"fa-solid fa-eye\"></i> Ver Avatar",
        "button[onclick='loadAvatarInfo()']": "<i class=\"fa-solid fa-download\"></i> Cargar Avatar",
        "button[onclick='clearInputFields()']": "<i class=\"fa-solid fa-eraser\"></i> Limpiar Campos",
        "#unlock-tab h2": "<i class=\"fa-solid fa-lock-open\"></i> Acciones de Desbloqueo",
        "#unlock-warning-panel p": "<i class=\"fa-solid fa-triangle-exclamation\"></i> Por favor, carga un avatar en la pestaña Avatar primero.",
        "button[onclick=\"triggerPopup('Unlock')\"]": "<i class=\"fa-solid fa-unlock\"></i> Desbloquear",
        "button[onclick=\"triggerPopup('Unlock')\"] + p": "Usará el método general para desbloquear avatares.",
        "button[onclick=\"triggerPopup('Unlock (VRCFury)')\"]": "<i class=\"fa-solid fa-bolt\"></i> Desbloquear (VRCFury)",
        "button[onclick=\"triggerPopup('Unlock (VRCFury)')\"] + p": "Intentará desbloquear parámetros de VRCFury.",
        "button[onclick=\"triggerPopup('Database Unlock')\"]": "<i class=\"fa-solid fa-database\"></i> Desbloqueo por Base de Datos",
        "button[onclick=\"triggerPopup('Database Unlock')\"] + p": "Escanea los bloqueos contra la base de datos SQL para encontrar coincidencias y desbloquear.",
        "button[onclick=\"triggerRestart('restart-novr')\"]": "<i class=\"fa-solid fa-arrows-rotate\"></i> Reiniciar",
        "button[onclick=\"triggerRestart('restart-novr')\"] + p": "Reinicia VRChat.",
        "button[onclick=\"triggerRestart('restart-vr')\"]": "<i class=\"fa-solid fa-arrows-rotate\"></i> Reiniciar (VR)",
        "button[onclick=\"triggerRestart('restart-vr')\"] + p": "Reinicia VRChat en modo VR.",
        "#loaded-avatar-info-panel h3": "Avatar Cargado",
        "#history-h3": "Historial",
        "#manage-history-h3": "Gestionar Historial",
        "#loaded-avi-id-label": "ID de Avatar:",
        "#loaded-user-id-label": "ID de Usuario:",
        "button[onclick=\"showUnloadConfirmation()\"]": "<i class=\"fa-solid fa-times-circle\"></i> Descargar Avatar",
        "#recents-tab h2": "<i class=\"fa-solid fa-history\"></i> Recientes",
        "button[onclick='exportHistory()']": "<i class=\"fa-solid fa-file-export\"></i> Exportar JSON",
        "button[onclick='showImportPopup()']": "<i class=\"fa-solid fa-file-import\"></i> Importar JSON",
        "#logs-tab h2": "<i class=\"fa-solid fa-file-lines\"></i> Registros",
        "button[onclick='clearAppLogs()']": "<i class=\"fa-solid fa-trash\"></i> Limpiar Registros",
        "button[onclick='copyAppLogs()']": "<i class=\"fa-solid fa-copy\"></i> Copiar",
        "#settings-tab h2": "<i class=\"fa-solid fa-gear\"></i> Ajustes",
        "label[for='animations-toggle']": "Activar animaciones de interfaz",
        "label[for='backend-messages-toggle']": "Mostrar mensajes de backend",
        "button[onclick='clearCacheAndReload()']": "<i class=\"fa-solid fa-trash-can\"></i> Limpiar Caché y Recargar",
        "#help-tab h2": "<i class=\"fa-solid fa-circle-question\"></i> Guía de Ayuda",
        "#help-yt-text": "¡Para un tutorial visual detallado, asegúrate de visitar mi canal de YouTube <strong>@ScrimDev</strong>!",
        "#help-step1-title": "<i class=\"fa-solid fa-clipboard-list\" style=\"margin-right:8px;\"></i> Paso 1: Preparación",
        "#help-step1-text": "Abre VRChat y cambia al avatar que deseas desbloquear. Espera a que el avatar cargue completamente (recomiendo esperar unos segundos extra para asegurar que esté en caché). Luego, introduce tu ID de Usuario de VRChat y el ID del Avatar en la aplicación.",
        "#help-step2-title": "<i class=\"fa-solid fa-lock-open\" style=\"margin-right:8px;\"></i> Paso 2: Desbloqueo",
        "#help-step2-text": "Intenta desbloquear el avatar usando el método <strong>Desbloquear</strong> estándar. Si el avatar utiliza bloqueos personalizados o VRCFury, usa <strong>Desbloquear (VRCFury)</strong> o <strong>Desbloqueo por Base de Datos</strong> en su lugar.",
        "#help-step3-title": "<i class=\"fa-solid fa-flag-checkered\" style=\"margin-right:8px;\"></i> Finalmente: Reiniciar",
        "#help-step3-text": "Reinicia tu juego usando los botones de reinicio dentro de la aplicación. ¡Disfruta de tu avatar recién desbloqueado!",
        "#info-p1": "AvatarLockpick",
        "#info-p2": "GUI hecha con Photino.NET",
        "#info-p3": "Desarrollado por Scrimmane / ScrimDev",
        "#info-p4": "Icono de App por Kmg Design",
        "#info-btn1": "Ver Registro de .NET",
        "#info-p5": "<strong>Nota:</strong> ¡Puedes personalizar esta interfaz! Siéntete libre de editar los archivos en la carpeta 'UI' de la aplicación para crear tu propio estilo.",
        "#info-btn2": "Página de Ayuda",
        "#settings-theme-h3": "Personalización del Tema",
        "#settings-presets-h4": "Preajustes",
        "#settings-preset-default": "Defecto (Azul)",
        "#settings-custom-colors-h4": "Colores Personalizados",
        "#settings-bg-label": "Color de Fondo:",
        "#settings-panel-label": "Panel/Fondo Secundario:",
        "#settings-text-label": "Color de Texto:",
        "#settings-accent-label": "Color de Acento:",
        "#settings-accent-hover-label": "Color de Desplazamiento de Acento:",
        "#settings-reset-theme": "Restablecer Tema",
        "#settings-prefs-h3": "Preferencias",
        "#settings-lang-h3": "Idioma",
        "#settings-lang-p": "Seleccione su idioma preferido.",
        "#settings-db-h3": "Base de Datos Personalizada",
        "#settings-db-p": "Cargue su propia base de datos de bloqueos desde una URL o ruta de archivo local. Déjelo vacío para usar la predeterminada.",
        "#settings-db-url-label": "URL de Base de Datos",
        "#settings-db-path-label": "O Ruta de Archivo Local",
        "#settings-db-save": "Guardar Ajustes",
        "#settings-db-reset": "Restablecer al Defecto",
        "#settings-adv-h3": "Avanzado",
        "#settings-adv-p1": "Borrar la caché restablecerá los ajustes de tema, el estado de la barra lateral y la preferencia de censura del ID de usuario almacenados en su navegador para esta GUI.",
        "#settings-adv-btn-clear": "Borrar Caché y Recargar GUI",
        "#settings-adv-btn-dir": "Abrir Directorio de App",
        "#confirm-popup-title": "¿Descargar Avatar?",
        "#confirm-popup-message": "¿Estás seguro de que deseas descargar la información del avatar actual?",
        "button[onclick='confirmUnloadAvatar()']": "Sí, Descargar",
        "button[onclick='cancelUnloadAvatar()']": "Cancelar",
        "#note-popup h3": "<i class=\"fa-solid fa-note-sticky\"></i> Editar Nota",
        "#note-popup p": "Añade o edita tu nota para el avatar a continuación.",
        "button[onclick='saveNote()']": "Guardar Nota",
        "button[onclick='closeNotePopup()']": "Cancelar",
        "#net-response-popup h3": "<i class=\"fa-solid fa-terminal\"></i> Registro de .NET",
        "button[onclick='closeNetResponsePopup()']": "Cerrar",
        "button[onclick='clearNetLog()']": "Limpiar Registro",
        "#import-json-popup h3": "Importar Historial desde JSON",
        "#import-json-popup p": "Pega tu contenido JSON a continuación. Esto sobrescribirá el historial actual.",
        "button[onclick='importHistory()']": "Importar",
        "button[onclick='closeImportPopup()']": "Cancelar",
        "#unlock-notice-popup h3": "<i class=\"fa-solid fa-info-circle\"></i> Aviso",
        "#unlock-notice-popup p": "La función 'Desbloqueo por Base de Datos' escanea el archivo del avatar contra una base de datos de firmas de bloqueos conocidas. Esto puede ayudar a desbloquear avatares incluso si están ocultos o renombrados.",
        "button[onclick='closeUnlockNotice()']": "Cerrar",
        "#download-title": "Descargando Base de Datos...",
        "#download-status": "Preparando descarga...",
        "#no-history-message": "No se han cargado avatares recientes. Carga uno en la pestaña 'Avatar' para verlo aquí.",
        "button[onclick='closePopup()']": "Cerrar"
    },
    "zh": {
        ".tab-button[data-tab='avatar'] .tab-text": "替身 (Avatar)",
        ".tab-button[data-tab='unlock'] .tab-text": "解锁",
        ".tab-button[data-tab='recents'] .tab-text": "最近",
        ".tab-button[data-tab='logs'] .tab-text": "日志",
        ".tab-button[data-tab='settings'] .tab-text": "设置",
        ".tab-button[data-tab='help'] .tab-text": "帮助",
        ".tab-button[data-tab='info'] .tab-text": "关于",
        "#avatar-tab h2": "<i class=\"fa-solid fa-user\"></i> 替身 (Avatar)",
        "label[for='avatar-id']": "替身 ID",
        "label[for='user-id']": "用户 ID",
        "label[for='censor-user-id']": "隐藏用户 ID",
        "button[onclick='callDotNetSendIDs()']": "<i class=\"fa-solid fa-eye\"></i> 查看替身",
        "button[onclick='loadAvatarInfo()']": "<i class=\"fa-solid fa-download\"></i> 加载替身",
        "button[onclick='clearInputFields()']": "<i class=\"fa-solid fa-eraser\"></i> 清除输入",
        "#unlock-tab h2": "<i class=\"fa-solid fa-lock-open\"></i> 解锁操作",
        "#unlock-warning-panel p": "<i class=\"fa-solid fa-triangle-exclamation\"></i> 请先在替身页面加载一个替身。",
        "button[onclick=\"triggerPopup('Unlock')\"]": "<i class=\"fa-solid fa-unlock\"></i> 解锁",
        "button[onclick=\"triggerPopup('Unlock')\"] + p": "使用通用方法解锁替身。",
        "button[onclick=\"triggerPopup('Unlock (VRCFury)')\"]": "<i class=\"fa-solid fa-bolt\"></i> 解锁 (VRCFury)",
        "button[onclick=\"triggerPopup('Unlock (VRCFury)')\"] + p": "尝试解锁 VRCFury 参数。",
        "button[onclick=\"triggerPopup('Database Unlock')\"]": "<i class=\"fa-solid fa-database\"></i> 数据库解锁",
        "button[onclick=\"triggerPopup('Database Unlock')\"] + p": "将替身的锁定值与 SQL 数据库进行比对以进行解锁。",
        "button[onclick=\"triggerRestart('restart-novr')\"]": "<i class=\"fa-solid fa-arrows-rotate\"></i> 重启",
        "button[onclick=\"triggerRestart('restart-novr')\"] + p": "重新启动 VRChat。",
        "button[onclick=\"triggerRestart('restart-vr')\"]": "<i class=\"fa-solid fa-arrows-rotate\"></i> 重启 (VR)",
        "button[onclick=\"triggerRestart('restart-vr')\"] + p": "在 VR 模式下重启 VRChat。",
        "#loaded-avatar-info-panel h3": "已加载替身",
        "#history-h3": "历史记录",
        "#manage-history-h3": "管理历史记录",
        "#loaded-avi-id-label": "替身 ID:",
        "#loaded-user-id-label": "用户 ID:",
        "button[onclick=\"showUnloadConfirmation()\"]": "<i class=\"fa-solid fa-times-circle\"></i> 卸载替身",
        "#recents-tab h2": "<i class=\"fa-solid fa-history\"></i> 最近加载",
        "button[onclick='exportHistory()']": "<i class=\"fa-solid fa-file-export\"></i> 导出 JSON",
        "button[onclick='showImportPopup()']": "<i class=\"fa-solid fa-file-import\"></i> 导入 JSON",
        "#logs-tab h2": "<i class=\"fa-solid fa-file-lines\"></i> 应用程序日志",
        "button[onclick='clearAppLogs()']": "<i class=\"fa-solid fa-trash\"></i> 清除日志",
        "button[onclick='copyAppLogs()']": "<i class=\"fa-solid fa-copy\"></i> 复制到剪贴板",
        "#settings-tab h2": "<i class=\"fa-solid fa-gear\"></i> 设置",
        "label[for='animations-toggle']": "启用 UI 动画",
        "label[for='backend-messages-toggle']": "显示后台通信弹出窗口",
        "button[onclick='clearCacheAndReload()']": "<i class=\"fa-solid fa-trash-can\"></i> 清除缓存并重新加载",
        "#help-tab h2": "<i class=\"fa-solid fa-circle-question\"></i> 帮助指南",
        "#help-yt-text": "欲了解详细的视觉教程，请务必前往我的 YouTube 频道 <strong>@ScrimDev</strong>！",
        "#help-step1-title": "<i class=\"fa-solid fa-clipboard-list\" style=\"margin-right:8px;\"></i> 第一步：准备",
        "#help-step1-text": "启动 VRChat 并切换到你想要解锁的替身。等待替身完全加载（我建议多等几秒钟以确保它完全缓存）。然后，将您的 VRChat 用户 ID 和替身 ID 输入到应用程序中。",
        "#help-step2-title": "<i class=\"fa-solid fa-lock-open\" style=\"margin-right:8px;\"></i> 第二步：解锁",
        "#help-step2-text": "尝试使用标准的 <strong>解锁</strong> 方法来解锁替身。如果替身使用了自定义锁定或 VRCFury，请使用 <strong>解锁 (VRCFury)</strong> 或 <strong>数据库解锁</strong>。",
        "#help-step3-title": "<i class=\"fa-solid fa-flag-checkered\" style=\"margin-right:8px;\"></i> 最后：重启",
        "#help-step3-text": "使用应用程序中的重启按钮重启游戏。享受你刚刚解锁的替身！",
        "#info-p1": "AvatarLockpick",
        "#info-p2": "使用 Photino.NET 制作的 GUI",
        "#info-p3": "由 Scrimmane / ScrimDev 开发",
        "#info-p4": "应用程序图标由 Kmg Design 提供",
        "#info-btn1": "查看 .NET 日志",
        "#info-p5": "<strong>注意：</strong> 您可以自定义此界面！随意编辑应用程序 'UI' 文件夹中的文件以创建您自己的风格。",
        "#info-btn2": "帮助页面",
        "#settings-theme-h3": "主题自定义",
        "#settings-presets-h4": "预设",
        "#settings-preset-default": "默认 (蓝色)",
        "#settings-custom-colors-h4": "自定义颜色",
        "#settings-bg-label": "背景颜色:",
        "#settings-panel-label": "面板/次要背景:",
        "#settings-text-label": "文本颜色:",
        "#settings-accent-label": "强调色:",
        "#settings-accent-hover-label": "强调色悬停:",
        "#settings-reset-theme": "重置主题",
        "#settings-prefs-h3": "偏好设置",
        "#settings-lang-h3": "语言",
        "#settings-lang-p": "选择您的首选语言。",
        "#settings-db-h3": "自定义数据库",
        "#settings-db-p": "从 URL 或本地文件路径加载您自己的锁定数据库。留空以使用默认数据库。",
        "#settings-db-url-label": "数据库 URL",
        "#settings-db-path-label": "或本地文件路径",
        "#settings-db-save": "保存数据库设置",
        "#settings-db-reset": "重置为默认",
        "#settings-adv-h3": "高级",
        "#settings-adv-p1": "清除缓存将重置此 GUI 的浏览器中存储的主题设置、侧边栏状态和用户 ID 隐藏偏好设置。",
        "#settings-adv-btn-clear": "清除缓存并重新加载 GUI",
        "#settings-adv-btn-dir": "打开应用程序目录",
        "#confirm-popup-title": "卸载替身？",
        "#confirm-popup-message": "您确定要卸载当前的替身信息吗？",
        "button[onclick='confirmUnloadAvatar()']": "是的，卸载",
        "button[onclick='cancelUnloadAvatar()']": "取消",
        "#note-popup h3": "<i class=\"fa-solid fa-note-sticky\"></i> 编辑备注",
        "#note-popup p": "在下方添加或编辑您对该替身的备注。",
        "button[onclick='saveNote()']": "保存备注",
        "button[onclick='closeNotePopup()']": "取消",
        "#net-response-popup h3": "<i class=\"fa-solid fa-terminal\"></i> .NET 日志",
        "button[onclick='closeNetResponsePopup()']": "关闭",
        "button[onclick='clearNetLog()']": "清除日志",
        "#import-json-popup h3": "从 JSON 导入历史记录",
        "#import-json-popup p": "在下方粘贴您的 JSON 内容。这将覆盖您当前的历史记录。",
        "button[onclick='importHistory()']": "导入",
        "button[onclick='closeImportPopup()']": "取消",
        "#unlock-notice-popup h3": "<i class=\"fa-solid fa-info-circle\"></i> 通知",
        "#unlock-notice-popup p": "“数据库解锁”功能会将替身文件与已知锁定签名的数据库进行对比。即使锁定组件被隐藏或重命名，这也可能有助于解锁替身。",
        "button[onclick='closeUnlockNotice()']": "关闭",
        "#download-title": "正在下载数据库...",
        "#download-status": "准备下载...",
        "#no-history-message": "没有最近加载的替身。在“替身”窗口中加载替身以在此处显示。",
        "button[onclick='closePopup()']": "关闭"
    },
    "ja": {
        ".tab-button[data-tab='avatar'] .tab-text": "アバター",
        ".tab-button[data-tab='unlock'] .tab-text": "アンロック",
        ".tab-button[data-tab='recents'] .tab-text": "履歴",
        ".tab-button[data-tab='logs'] .tab-text": "ログ",
        ".tab-button[data-tab='settings'] .tab-text": "設定",
        ".tab-button[data-tab='help'] .tab-text": "ヘルプ",
        ".tab-button[data-tab='info'] .tab-text": "情報",
        "#avatar-tab h2": "<i class=\"fa-solid fa-user\"></i> アバター",
        "label[for='avatar-id']": "アバター ID",
        "label[for='user-id']": "ユーザー ID",
        "label[for='censor-user-id']": "ユーザー ID を隠す",
        "button[onclick='callDotNetSendIDs()']": "<i class=\"fa-solid fa-eye\"></i> アバターを見る",
        "button[onclick='loadAvatarInfo()']": "<i class=\"fa-solid fa-download\"></i> アバターを読み込む",
        "button[onclick='clearInputFields()']": "<i class=\"fa-solid fa-eraser\"></i> 入力をクリア",
        "#unlock-tab h2": "<i class=\"fa-solid fa-lock-open\"></i> アンロック アクション",
        "#unlock-warning-panel p": "<i class=\"fa-solid fa-triangle-exclamation\"></i> まずアバター タブでアバターを読み込んでください。",
        "button[onclick=\"triggerPopup('Unlock')\"]": "<i class=\"fa-solid fa-unlock\"></i> アンロック",
        "button[onclick=\"triggerPopup('Unlock')\"] + p": "一般的な方法を使用してアバターをアンロックします。",
        "button[onclick=\"triggerPopup('Unlock (VRCFury)')\"]": "<i class=\"fa-solid fa-bolt\"></i> アンロック (VRCFury)",
        "button[onclick=\"triggerPopup('Unlock (VRCFury)')\"] + p": "VRCFury パラメータのアンロックを試みます。",
        "button[onclick=\"triggerPopup('Database Unlock')\"]": "<i class=\"fa-solid fa-database\"></i> データベース アンロック",
        "button[onclick=\"triggerPopup('Database Unlock')\"] + p": "SQL データベースと照合して一致するものを見つけ、アンロックします。",
        "button[onclick=\"triggerRestart('restart-novr')\"]": "<i class=\"fa-solid fa-arrows-rotate\"></i> 再起動",
        "button[onclick=\"triggerRestart('restart-novr')\"] + p": "VRChat を再起動します。",
        "button[onclick=\"triggerRestart('restart-vr')\"]": "<i class=\"fa-solid fa-arrows-rotate\"></i> 再起動 (VR)",
        "button[onclick=\"triggerRestart('restart-vr')\"] + p": "VR モードで VRChat を再起動します。",
        "#loaded-avatar-info-panel h3": "読み込まれたアバター",
        "#history-h3": "履歴",
        "#manage-history-h3": "履歴の管理",
        "#loaded-avi-id-label": "アバター ID:",
        "#loaded-user-id-label": "ユーザー ID:",
        "button[onclick=\"showUnloadConfirmation()\"]": "<i class=\"fa-solid fa-times-circle\"></i> アバターをアンロードする",
        "#recents-tab h2": "<i class=\"fa-solid fa-history\"></i> 履歴",
        "button[onclick='exportHistory()']": "<i class=\"fa-solid fa-file-export\"></i> JSON でエクスポート",
        "button[onclick='showImportPopup()']": "<i class=\"fa-solid fa-file-import\"></i> JSON をインポート",
        "#logs-tab h2": "<i class=\"fa-solid fa-file-lines\"></i> ログ",
        "button[onclick='clearAppLogs()']": "<i class=\"fa-solid fa-trash\"></i> ログをクリア",
        "button[onclick='copyAppLogs()']": "<i class=\"fa-solid fa-copy\"></i> コピー",
        "#settings-tab h2": "<i class=\"fa-solid fa-gear\"></i> 設定",
        "label[for='animations-toggle']": "UI アニメーションを有効にする",
        "label[for='backend-messages-toggle']": "バックエンド通信を表示",
        "button[onclick='clearCacheAndReload()']": "<i class=\"fa-solid fa-trash-can\"></i> キャッシュをクリアして再読み込み",
        "#help-tab h2": "<i class=\"fa-solid fa-circle-question\"></i> ヘルプガイド",
        "#help-yt-text": "詳細なビジュアルチュートリアルについては、私の YouTube チャンネル <strong>@ScrimDev</strong> をぜひご覧ください！",
        "#help-step1-title": "<i class=\"fa-solid fa-clipboard-list\" style=\"margin-right:8px;\"></i> ステップ 1: 準備",
        "#help-step1-text": "VRChatを起動し、アンロックしたいアバターに変更します。アバターが完全にロードされるまで待ちます（完全にキャッシュされるよう数秒余分に待つことをお勧めします）。その後、VRChatのユーザーIDとアバターのIDをアプリケーションに入力してください。",
        "#help-step2-title": "<i class=\"fa-solid fa-lock-open\" style=\"margin-right:8px;\"></i> ステップ 2: アンロック",
        "#help-step2-text": "標準の <strong>アンロック</strong> メソッドを使用して、アバターのアンロックを試みます。アバターにカスタムロックまたはVRCFuryロックが使用されている場合は、代わりに <strong>アンロック (VRCFury)</strong> または <strong>データベース アンロック</strong> を使用してください。",
        "#help-step3-title": "<i class=\"fa-solid fa-flag-checkered\" style=\"margin-right:8px;\"></i> 最後に: 再起動",
        "#help-step3-text": "アプリ内の再起動ボタンを使用してゲームを再起動します。新しくアンロックされたアバターをお楽しみください！",
        "#info-p1": "AvatarLockpick",
        "#info-p2": "Photino.NET で作成された GUI",
        "#info-p3": "Scrimmane / ScrimDev 開発",
        "#info-p4": "アプリアイコン: Kmg Design",
        "#info-btn1": ".NET ログを表示",
        "#info-p5": "<strong>注意：</strong> このインターフェースはカスタマイズ可能です！独自のスタイルを作成するために、アプリケーションの「UI」フォルダー内のファイルを自由に編集してください。",
        "#info-btn2": "ヘルプページ",
        "#settings-theme-h3": "テーマのカスタマイズ",
        "#settings-presets-h4": "プリセット",
        "#settings-preset-default": "デフォルト (ブルー)",
        "#settings-custom-colors-h4": "カスタムカラー",
        "#settings-bg-label": "背景色:",
        "#settings-panel-label": "パネル/セカンダリ背景:",
        "#settings-text-label": "テキスト色:",
        "#settings-accent-label": "アクセント色:",
        "#settings-accent-hover-label": "アクセント色（ホバー時）:",
        "#settings-reset-theme": "テーマをリセット",
        "#settings-prefs-h3": "環境設定",
        "#settings-lang-h3": "言語",
        "#settings-lang-p": "使用する言語を選択してください。",
        "#settings-db-h3": "カスタムデータベース",
        "#settings-db-p": "URL またはローカルファイルパスから独自のロックデータベースをロードします。デフォルトを使用するには空のままにしてください。",
        "#settings-db-url-label": "データベース URL",
        "#settings-db-path-label": "またはローカルファイルパス",
        "#settings-db-save": "データベース設定を保存",
        "#settings-db-reset": "デフォルトにリセット",
        "#settings-adv-h3": "高度な設定",
        "#settings-adv-p1": "キャッシュをクリアすると、この GUI のブラウザに保存されているテーマ設定、サイドバーの状態、およびユーザー ID 非表示の環境設定がリセットされます。",
        "#settings-adv-btn-clear": "キャッシュをクリアして GUI を再ロード",
        "#settings-adv-btn-dir": "アプリ ディレクトリを開く",
        "#confirm-popup-title": "アバターをアンロードしますか？",
        "#confirm-popup-message": "現在のアバター情報をアンロードしてもよろしいですか？",
        "button[onclick='confirmUnloadAvatar()']": "はい、アンロードします",
        "button[onclick='cancelUnloadAvatar()']": "キャンセル",
        "#note-popup h3": "<i class=\"fa-solid fa-note-sticky\"></i> メモの編集",
        "#note-popup p": "以下のフォームからアバターのメモを追加または編集してください。",
        "button[onclick='saveNote()']": "メモを保存",
        "button[onclick='closeNotePopup()']": "キャンセル",
        "#net-response-popup h3": "<i class=\"fa-solid fa-terminal\"></i> .NET ログ",
        "button[onclick='closeNetResponsePopup()']": "閉じる",
        "button[onclick='clearNetLog()']": "ログを消去",
        "#import-json-popup h3": "JSON から履歴をインポート",
        "#import-json-popup p": "以下のフォームに JSON コンテンツを貼り付けてください。現在の履歴が上書きされます。",
        "button[onclick='importHistory()']": "インポート",
        "button[onclick='closeImportPopup()']": "キャンセル",
        "#unlock-notice-popup h3": "<i class=\"fa-solid fa-info-circle\"></i> 通知",
        "#unlock-notice-popup p": "「データベースアンロック」機能は、既知のロックシグネチャのデータベースに対してアバターファイルをスキャンします。これにより、ロックコンポーネントが隠されていてもアバターのアンロックが行えます。",
        "button[onclick='closeUnlockNotice()']": "閉じる",
        "#download-title": "データベースをダウンロード中...",
        "#download-status": "ダウンロードの準備中...",
        "#no-history-message": "最近読み込まれたアバターはありません。ここで表示するには、「アバター」タブでアバターを読み込んでください。",
        "button[onclick='closePopup()']": "閉じる"
    },
    "ru": {
        ".tab-button[data-tab='avatar'] .tab-text": "Аватар",
        ".tab-button[data-tab='unlock'] .tab-text": "Разблокировать",
        ".tab-button[data-tab='recents'] .tab-text": "Недавние",
        ".tab-button[data-tab='logs'] .tab-text": "Журналы",
        ".tab-button[data-tab='settings'] .tab-text": "Настройки",
        ".tab-button[data-tab='help'] .tab-text": "Помощь",
        ".tab-button[data-tab='info'] .tab-text": "Инфо",
        "#avatar-tab h2": "<i class=\"fa-solid fa-user\"></i> Аватар",
        "label[for='avatar-id']": "ID Аватара",
        "label[for='user-id']": "ID Пользователя",
        "label[for='censor-user-id']": "Скрыть ID Пользователя",
        "button[onclick='callDotNetSendIDs()']": "<i class=\"fa-solid fa-eye\"></i> Посмотреть Аватар",
        "button[onclick='loadAvatarInfo()']": "<i class=\"fa-solid fa-download\"></i> Загрузить Аватар",
        "button[onclick='clearInputFields()']": "<i class=\"fa-solid fa-eraser\"></i> Очистить Поля",
        "#unlock-tab h2": "<i class=\"fa-solid fa-lock-open\"></i> Действия Разблокировки",
        "#unlock-warning-panel p": "<i class=\"fa-solid fa-triangle-exclamation\"></i> Пожалуйста, сначала загрузите аватар на вкладке Аватар.",
        "button[onclick=\"triggerPopup('Unlock')\"]": "<i class=\"fa-solid fa-unlock\"></i> Разблокировать",
        "button[onclick=\"triggerPopup('Unlock')\"] + p": "Использовать общий метод для разблокировки аватаров.",
        "button[onclick=\"triggerPopup('Unlock (VRCFury)')\"]": "<i class=\"fa-solid fa-bolt\"></i> Разблокировать (VRCFury)",
        "button[onclick=\"triggerPopup('Unlock (VRCFury)')\"] + p": "Попытает разблокировать параметры VRCFury.",
        "button[onclick=\"triggerPopup('Database Unlock')\"]": "<i class=\"fa-solid fa-database\"></i> Разблокировка БД",
        "button[onclick=\"triggerPopup('Database Unlock')\"] + p": "Сканирует блокировки на совпадение с SQL-базой данных.",
        "button[onclick=\"triggerRestart('restart-novr')\"]": "<i class=\"fa-solid fa-arrows-rotate\"></i> Перезапуск",
        "button[onclick=\"triggerRestart('restart-novr')\"] + p": "Перезапускает VRChat.",
        "button[onclick=\"triggerRestart('restart-vr')\"]": "<i class=\"fa-solid fa-arrows-rotate\"></i> Перезапуск (VR)",
        "button[onclick=\"triggerRestart('restart-vr')\"] + p": "Перезапускает VRChat в VR.",
        "#loaded-avatar-info-panel h3": "Загруженный Аватар",
        "#history-h3": "История",
        "#manage-history-h3": "Управление Историей",
        "#loaded-avi-id-label": "ID Аватара:",
        "#loaded-user-id-label": "ID Пользователя:",
        "button[onclick=\"showUnloadConfirmation()\"]": "<i class=\"fa-solid fa-times-circle\"></i> Выгрузить Аватар",
        "#recents-tab h2": "<i class=\"fa-solid fa-history\"></i> Недавние",
        "button[onclick='exportHistory()']": "<i class=\"fa-solid fa-file-export\"></i> Экспорт JSON",
        "button[onclick='showImportPopup()']": "<i class=\"fa-solid fa-file-import\"></i> Импорт JSON",
        "#logs-tab h2": "<i class=\"fa-solid fa-file-lines\"></i> Журналы Приложения",
        "button[onclick='clearAppLogs()']": "<i class=\"fa-solid fa-trash\"></i> Очистить Журналы",
        "button[onclick='copyAppLogs()']": "<i class=\"fa-solid fa-copy\"></i> Скопировать",
        "#settings-tab h2": "<i class=\"fa-solid fa-gear\"></i> Настройки",
        "label[for='animations-toggle']": "Включить Анимацию UI",
        "label[for='backend-messages-toggle']": "Показывать сообщения бэкенда",
        "button[onclick='clearCacheAndReload()']": "<i class=\"fa-solid fa-trash-can\"></i> Очистить Кэш и Перезагрузить GUI",
        "#help-tab h2": "<i class=\"fa-solid fa-circle-question\"></i> Руководство",
        "#help-yt-text": "Для подробного видеоурока обязательно зайдите на мой YouTube-канал <strong>@ScrimDev</strong>!",
        "#help-step1-title": "<i class=\"fa-solid fa-clipboard-list\" style=\"margin-right:8px;\"></i> Шаг 1: Подготовка",
        "#help-step1-text": "Запустите VRChat и переключитесь на аватар, который хотите разблокировать. Дождитесь полной загрузки аватара (я рекомендую подождать несколько лишних секунд, чтобы убедиться, что он закэшировался). Затем введите ваш User ID VRChat и ID Аватара в приложение.",
        "#help-step2-title": "<i class=\"fa-solid fa-lock-open\" style=\"margin-right:8px;\"></i> Шаг 2: Разблокировка",
        "#help-step2-text": "Попробуйте разблокировать аватар с помощью стандартного метода <strong>Разблокировать</strong>. Если у аватара есть кастомные или VRCFury блокировки, используйте <strong>Разблокировать (VRCFury)</strong> или <strong>Разблокировка БД</strong>.",
        "#help-step3-title": "<i class=\"fa-solid fa-flag-checkered\" style=\"margin-right:8px;\"></i> Наконец: Перезапуск",
        "#help-step3-text": "Перезапустите игру с помощью кнопок перезапуска в приложении. Наслаждайтесь своим разблокированным аватаром!",
        "#info-p1": "AvatarLockpick",
        "#info-p2": "GUI сделан на Photino.NET",
        "#info-p3": "Разработчик Scrimmane / ScrimDev",
        "#info-p4": "Иконка приложения: Kmg Design",
        "#info-btn1": "Посмотреть .NET лог",
        "#info-p5": "<strong>Примечание:</strong> Вы можете настроить этот интерфейс! Смело редактируйте файлы в папке 'UI' приложения, чтобы создать свой собственный стиль.",
        "#info-btn2": "Страница помощи",
        "#settings-theme-h3": "Настройка темы",
        "#settings-presets-h4": "Пресеты",
        "#settings-preset-default": "По умолчанию (Синий)",
        "#settings-custom-colors-h4": "Свои цвета",
        "#settings-bg-label": "Цвет фона:",
        "#settings-panel-label": "Панель/Вторичный фон:",
        "#settings-text-label": "Цвет текста:",
        "#settings-accent-label": "Цвет акцента:",
        "#settings-accent-hover-label": "Цвет наведения акцента:",
        "#settings-reset-theme": "Сбросить тему",
        "#settings-prefs-h3": "Настройки",
        "#settings-lang-h3": "Язык",
        "#settings-lang-p": "Выберите предпочитаемый язык.",
        "#settings-db-h3": "Пользовательская база данных",
        "#settings-db-p": "Загрузите свою собственную базу данных блокировок по URL-адресу или локальному пути. Оставьте пустым, чтобы использовать по умолчанию.",
        "#settings-db-url-label": "URL базы данных",
        "#settings-db-path-label": "Или Локальный Путь",
        "#settings-db-save": "Сохранить настройки базы данных",
        "#settings-db-reset": "Сбросить по умолчанию",
        "#settings-adv-h3": "Расширенные",
        "#settings-adv-p1": "Очистка кэша сбросит настройки темы, состояние боковой панели и предпочтение цензуры идентификатора пользователя, сохраненные в вашем браузере для этого GUI.",
        "#settings-adv-btn-clear": "Очистить кэш и перезагрузить GUI",
        "#settings-adv-btn-dir": "Открыть директорию приложения",
        "#confirm-popup-title": "Выгрузить Аватар?",
        "#confirm-popup-message": "Вы уверены, что хотите выгрузить информацию о текущем аватаре?",
        "button[onclick='confirmUnloadAvatar()']": "Да, Выгрузить",
        "button[onclick='cancelUnloadAvatar()']": "Отмена",
        "#note-popup h3": "<i class=\"fa-solid fa-note-sticky\"></i> Редактировать Заметку",
        "#note-popup p": "Добавьте или отредактируйте вашу заметку для аватара ниже.",
        "button[onclick='saveNote()']": "Сохранить",
        "button[onclick='closeNotePopup()']": "Отмена",
        "#net-response-popup h3": "<i class=\"fa-solid fa-terminal\"></i> Журнал .NET",
        "button[onclick='closeNetResponsePopup()']": "Закрыть",
        "button[onclick='clearNetLog()']": "Очистить Журнал",
        "#import-json-popup h3": "Импорт истории из JSON",
        "#import-json-popup p": "Вставьте ваш JSON-контент ниже. Это перезапишет вашу текущую историю.",
        "button[onclick='importHistory()']": "Импорт",
        "button[onclick='closeImportPopup()']": "Отмена",
        "#unlock-notice-popup h3": "<i class=\"fa-solid fa-info-circle\"></i> Уведомление",
        "#unlock-notice-popup p": "Функция 'Разблокировка БД' сканирует файл аватара по базе данных известных сигнатур блокировок. Это может помочь разблокировать аватары, даже если компоненты блокировки скрыты, переименованы или зашифрованы.",
        "button[onclick='closeUnlockNotice()']": "Закрыть",
        "#download-title": "Загрузка базы данных...",
        "#download-status": "Подготовка к загрузке...",
        "#no-history-message": "Недавно загруженных аватаров нет. Загрузите аватар во вкладке 'Аватар', чтобы увидеть его здесь.",
        "button[onclick='closePopup()']": "Закрыть"
    }
};

let currentLang = 'en';

function updateLanguage(lang) {
    if (!translations[lang]) lang = 'en';
    currentLang = lang;
    localStorage.setItem('appLanguage', lang);
    const langDict = translations[lang];
    
    for (const selector in langDict) {
        const el = document.querySelector(selector);
        if (el) {
            el.innerHTML = langDict[selector];
        }
    }
}
function openAppDir() { sendMessageToDotNet({ action: 'openAppDirectory' }); }
