// --- Explorer Extension: Titlebar, Tabs, Avatar Info, Settings ---------------

window.addEventListener('load', () => {
    initTitlebar();
    initTabs();
});

// --- Titlebar Drag ------------------------------------------------------------
function initTitlebar() {
    const header = document.getElementById('exp-header');
    if (!header) return;
    header.id = 'exp-titlebar';

    const left = document.getElementById('exp-header-left');
    if (left) left.id = 'exp-titlebar-drag';

    const right = document.getElementById('exp-header-right');
    if (right) {
        right.id = 'exp-titlebar-controls';
        right.innerHTML =
            '<button class="exp-tbtn" onclick="expMinimize()" title="Minimize"><i class="fa-solid fa-minus"></i></button>' +
            '<button class="exp-tbtn exp-tbtn-close" onclick="goBack()" title="Back to Main"><i class="fa-solid fa-xmark"></i></button>';
    }

    const drag = document.getElementById('exp-titlebar-drag');
    if (!drag) return;
    let dragging = false, lastX = 0, lastY = 0;
    drag.addEventListener('mousedown', e => {
        if (e.button !== 0 || e.target.closest('.exp-tbtn, .exp-btn')) return;
        dragging = true; lastX = e.screenX; lastY = e.screenY; e.preventDefault();
    });
    document.addEventListener('mousemove', e => {
        if (!dragging) return;
        const dx = e.screenX - lastX, dy = e.screenY - lastY;
        if (!dx && !dy) return;
        lastX = e.screenX; lastY = e.screenY;
        sendMsg({ type: 'windowControl', cmd: 'move', dx, dy });
    });
    document.addEventListener('mouseup', () => { dragging = false; });
}

function expMinimize() { sendMsg({ type: 'windowControl', cmd: 'minimize' }); }

// --- Tab Setup ----------------------------------------------------------------
function initTabs() {
    // Insert tab nav bar before the status bar
    const statusBar = document.getElementById('exp-status-bar');
    if (!statusBar) return;
    const navBar = document.createElement('div');
    navBar.id = 'exp-nav-bar';
    navBar.innerHTML =
        '<div id="exp-tab-list">' +
            '<button class="exp-tab-btn active" onclick="switchTab(\'parameters\',this)"><i class="fa-solid fa-list-check"></i> Parameters</button>' +
            '<button class="exp-tab-btn" onclick="switchTab(\'avatar-info\',this)"><i class="fa-solid fa-id-card"></i> Avatar Info</button>' +
            '<button class="exp-tab-btn" onclick="switchTab(\'settings\',this)"><i class="fa-solid fa-sliders"></i> Settings</button>' +
        '</div>' +
        '<div id="exp-nav-actions">' +
            '<button class="exp-btn exp-btn-secondary" onclick="refreshData()"><i class="fa-solid fa-rotate"></i> Refresh</button>' +
        '</div>';
    statusBar.parentNode.insertBefore(navBar, statusBar);
    const tabList = document.getElementById('exp-tab-list');
    if (tabList) {
        const helpBtn = document.createElement('button');
        helpBtn.className = 'exp-tab-btn';
        helpBtn.setAttribute('onclick', "switchTab('help',this)");
        helpBtn.innerHTML = '<i class="fa-solid fa-circle-question"></i> Help';
        tabList.appendChild(helpBtn);
    }

    // Wrap existing main content in tab-parameters panel
    const main = document.getElementById('exp-main');
    if (!main) return;
    const paramPanel = document.createElement('div');
    paramPanel.id = 'tab-parameters';
    paramPanel.className = 'exp-tab-panel active';
    while (main.firstChild) paramPanel.appendChild(main.firstChild);
    main.appendChild(paramPanel);

    // Avatar Info tab panel
    const aviPanel = document.createElement('div');
    aviPanel.id = 'tab-avatar-info';
    aviPanel.className = 'exp-tab-panel';
    aviPanel.innerHTML =
        '<div id="avi-loading" class="tab-center-msg">' +
            '<i class="fa-solid fa-spinner fa-spin" style="font-size:2em;color:var(--accent-color);"></i>' +
            '<p style="margin-top:12px;opacity:0.6;">Waiting for data...</p>' +
        '</div>' +
        '<div id="avi-content" class="info-scroll" style="display:none;">' +
            '<div class="info-section">' +
                '<h3 class="info-section-title"><i class="fa-solid fa-fingerprint"></i> Identifiers</h3>' +
                '<div class="info-grid">' +
                    '<div class="info-row"><span class="info-label">Avatar ID</span><span class="info-val mono" id="ii-avatar-id">—</span><button class="exp-copy-btn" onclick="copyText(document.getElementById(\'ii-avatar-id\').textContent)"><i class="fa-solid fa-copy"></i></button></div>' +
                    '<div class="info-row"><span class="info-label">User ID</span><span class="info-val mono" id="ii-user-id">—</span><button class="exp-copy-btn" onclick="copyText(document.getElementById(\'ii-user-id\').textContent)"><i class="fa-solid fa-copy"></i></button></div>' +
                    '<div class="info-row"><span class="info-label">OSC Name</span><span class="info-val" id="ii-osc-name">—</span></div>' +
                    '<div class="info-row"><span class="info-label">OSC ID</span><span class="info-val mono" id="ii-osc-id">—</span></div>' +
                '</div>' +
            '</div>' +
            '<div class="info-section">' +
                '<h3 class="info-section-title"><i class="fa-solid fa-folder-open"></i> File Paths</h3>' +
                '<div class="info-grid">' +
                    '<div class="info-row-col"><span class="info-label">OSC Config</span><span class="info-val mono small" id="ii-osc-path">—</span><span id="ii-osc-badge" class="exp-badge" style="margin-top:4px;"></span></div>' +
                    '<div class="info-row-col"><span class="info-label">Avatar Data</span><span class="info-val mono small" id="ii-avi-path">—</span><span id="ii-avi-badge" class="exp-badge" style="margin-top:4px;"></span></div>' +
                '</div>' +
            '</div>' +
            '<div class="info-section">' +
                '<h3 class="info-section-title"><i class="fa-solid fa-chart-bar"></i> Parameter Stats</h3>' +
                '<div class="stat-grid">' +
                    '<div class="stat-card"><span class="stat-num" id="ii-stat-total">—</span><span class="stat-lbl">Total</span></div>' +
                    '<div class="stat-card accent-card"><span class="stat-num" id="ii-stat-matched">—</span><span class="stat-lbl">Matched</span></div>' +
                    '<div class="stat-card warn-card"><span class="stat-num" id="ii-stat-osc">—</span><span class="stat-lbl">OSC Only</span></div>' +
                    '<div class="stat-card muted-card"><span class="stat-num" id="ii-stat-avi">—</span><span class="stat-lbl">No OSC</span></div>' +
                '</div>' +
            '</div>' +
        '</div>';
    main.appendChild(aviPanel);

    // Settings tab panel
    const settPanel = document.createElement('div');
    settPanel.id = 'tab-settings';
    settPanel.className = 'exp-tab-panel';
    settPanel.innerHTML =
        '<div class="info-scroll">' +
            '<div class="settings-block">' +
                '<h3 class="info-section-title"><i class="fa-solid fa-rotate"></i> Refresh</h3>' +
                '<p class="settings-desc">Re-read the OSC config and avatar data files from disk.</p>' +
                '<button class="exp-btn exp-btn-primary" onclick="refreshData()"><i class="fa-solid fa-rotate"></i> Refresh Data</button>' +
            '</div>' +
            '<div class="settings-block">' +
                '<h3 class="info-section-title" style="color:#f39c12;"><i class="fa-solid fa-file-code"></i> OSC Config File</h3>' +
                '<p class="settings-desc">Delete the OSC parameter config for this avatar. VRChat regenerates it next time the avatar loads. Does <strong>not</strong> affect the avatar data file.</p>' +
                '<div id="osc-path-preview" class="path-preview"></div>' +
                '<button class="exp-btn exp-btn-danger" onclick="confirmResetOsc()"><i class="fa-solid fa-trash"></i> Delete OSC Config</button>' +
                '<div id="osc-reset-result" class="settings-result" style="display:none;"></div>' +
            '</div>' +
            '<div class="settings-block">' +
                '<h3 class="info-section-title" style="color:#e74c3c;"><i class="fa-solid fa-database"></i> Avatar Cache</h3>' +
                '<p class="settings-desc">Delete the locally cached avatar data file from LocalAvatarData. Removes saved parameter values. Use with caution.</p>' +
                '<div id="avi-path-preview" class="path-preview"></div>' +
                '<button class="exp-btn exp-btn-danger" onclick="confirmResetAvatarCache()"><i class="fa-solid fa-trash"></i> Delete Avatar Cache</button>' +
                '<div id="avi-reset-result" class="settings-result" style="display:none;"></div>' +
            '</div>' +
        '</div>';
    main.appendChild(settPanel);

    const helpPanel = document.createElement('div');
    helpPanel.id = 'tab-help';
    helpPanel.className = 'exp-tab-panel';
    helpPanel.innerHTML =
        '<div class="info-scroll">' +
        '<div class="info-section">' +
        '<h3 class="info-section-title"><i class="fa-solid fa-compass"></i> What is Explorer?</h3>' +
        '<p class="help-p">Reads your VRChat OSC config and local avatar cache, compares them side by side, and lets you send custom OSC commands directly to VRChat.</p>' +
        '</div>' +
        '<div class="info-section">' +
        '<h3 class="info-section-title"><i class="fa-solid fa-tags"></i> Parameter Statuses</h3>' +
        '<div class="help-list">' +
        '<div class="help-row"><span class="exp-status-badge status-matched"><i class="fa-solid fa-circle-check"></i> Synced</span><span class="help-desc">Found in both the OSC config and avatar data. Fully registered and ready.</span></div>' +
        '<div class="help-row"><span class="exp-status-badge status-osc-only"><i class="fa-solid fa-triangle-exclamation"></i> OSC Only</span><span class="help-desc">In the OSC config but not avatar data. May be leftover from an older avatar version.</span></div>' +
        '<div class="help-row"><span class="exp-status-badge status-avatar-only"><i class="fa-solid fa-circle-question"></i> No OSC</span><span class="help-desc">In avatar data but has no OSC config entry. VRChat has not generated a mapping yet.</span></div>' +
        '</div>' +
        '</div>' +
        '<div class="info-section">' +
        '<h3 class="info-section-title"><i class="fa-solid fa-shapes"></i> Parameter Types</h3>' +
        '<div class="help-list">' +
        '<div class="help-row"><span class="exp-type-badge type-float">Float</span><span class="help-desc">Decimal value (0.0 to 1.0). Used for smooth transitions like blend shapes or animation speed.</span></div>' +
        '<div class="help-row"><span class="exp-type-badge type-int">Int</span><span class="help-desc">Whole number (0, 1, 2...). Used for gesture indices, expression states, etc.</span></div>' +
        '<div class="help-row"><span class="exp-type-badge type-bool">Bool</span><span class="help-desc">True or False. Used for on/off toggles and action triggers.</span></div>' +
        '</div>' +
        '</div>' +
        '<div class="info-section">' +
        '<h3 class="info-section-title"><i class="fa-solid fa-paper-plane"></i> Sending OSC Commands</h3>' +
        '<p class="help-p">Enter a value in the <strong>Custom Value</strong> column and click <strong>Send</strong>. Sent via UDP to <span class="help-code">127.0.0.1:9000</span>.</p>' +
        '<div class="help-note"><i class="fa-solid fa-circle-info"></i> VRChat must be running with OSC enabled. Go to <strong>VRChat Settings &gt; OSC &gt; Enable</strong>.</div>' +
        '</div>' +
        '<div class="info-section">' +
        '<h3 class="info-section-title"><i class="fa-solid fa-table-columns"></i> Tabs Overview</h3>' +
        '<div class="help-list">' +
        '<div class="help-row"><strong>Parameters</strong><span class="help-desc">Browse, filter, and send OSC commands for all avatar parameters.</span></div>' +
        '<div class="help-row"><strong>Avatar Info</strong><span class="help-desc">View avatar IDs, file paths, file status, and parameter statistics.</span></div>' +
        '<div class="help-row"><strong>Settings</strong><span class="help-desc">Delete the OSC config or avatar cache file for this avatar. Both are regenerated by VRChat on next load.</span></div>' +
        '</div>' +
        '</div>' +
        '</div>';
    main.appendChild(helpPanel);

    // Confirm dialog overlay
    const overlay = document.createElement('div');
    overlay.id = 'exp-confirm-overlay';
    overlay.style.cssText = 'display:none;position:fixed;inset:0;background:rgba(0,0,0,0.65);z-index:500;align-items:center;justify-content:center;';
    overlay.innerHTML =
        '<div class="exp-confirm-box">' +
            '<div style="font-size:2em;color:#f39c12;margin-bottom:10px;"><i class="fa-solid fa-triangle-exclamation"></i></div>' +
            '<h3 id="exp-confirm-title" style="margin:0 0 8px;font-size:1em;"></h3>' +
            '<p id="exp-confirm-msg" style="font-size:0.85em;opacity:0.75;margin:0 0 18px;line-height:1.5;"></p>' +
            '<div style="display:flex;gap:10px;justify-content:center;">' +
                '<button class="exp-btn exp-btn-secondary" onclick="closeConfirm()">Cancel</button>' +
                '<button class="exp-btn exp-btn-danger" id="exp-confirm-ok">Delete</button>' +
            '</div>' +
        '</div>';
    document.body.appendChild(overlay);
}

// --- Tab Switching ------------------------------------------------------------
function switchTab(tabId, btn) {
    document.querySelectorAll('.exp-tab-btn').forEach(b => b.classList.remove('active'));
    document.querySelectorAll('.exp-tab-panel').forEach(p => p.classList.remove('active'));
    btn.classList.add('active');
    const panel = document.getElementById('tab-' + tabId);
    if (panel) panel.classList.add('active');
    const isParams = tabId === 'parameters';
    const sb = document.getElementById('exp-status-bar');
    const fb = document.getElementById('exp-filter-bar');
    if (sb) sb.style.display = isParams ? 'flex' : 'none';
    if (fb) fb.style.display = isParams ? 'flex' : 'none';
}

// --- Avatar Info Rendering ----------------------------------------------------
function renderAvatarInfo(data) {
    setEl('ii-avatar-id', data.avatarId || '-');
    setEl('ii-user-id',   data.userId   || '-');
    setEl('ii-osc-name',  data.oscName  || '-');
    setEl('ii-osc-id',    data.oscId    || '-');
    setEl('ii-osc-path',  data.oscPath  || '-');
    setEl('ii-avi-path',  data.avatarPath || '-');

    const oscB = document.getElementById('ii-osc-badge');
    const aviB = document.getElementById('ii-avi-badge');
    if (oscB) { oscB.textContent = data.oscExists ? 'Found' : 'Not Found'; oscB.className = 'exp-badge ' + (data.oscExists ? 'exp-badge-green' : 'exp-badge-red'); }
    if (aviB) { aviB.textContent = data.avatarExists ? 'Found' : 'Not Found'; aviB.className = 'exp-badge ' + (data.avatarExists ? 'exp-badge-green' : 'exp-badge-red'); }

    const p = data.parameters || [];
    setEl('ii-stat-total',   p.length);
    setEl('ii-stat-matched', p.filter(x => x.status === 'matched').length);
    setEl('ii-stat-osc',     p.filter(x => x.status === 'osc-only').length);
    setEl('ii-stat-avi',     p.filter(x => x.status === 'avatar-only').length);

    const loading = document.getElementById('avi-loading');
    const content = document.getElementById('avi-content');
    if (loading) loading.style.display = 'none';
    if (content) content.style.display = 'block';
}

function renderSettingsPaths(data) {
    const oscPrev = document.getElementById('osc-path-preview');
    const aviPrev = document.getElementById('avi-path-preview');
    if (oscPrev) oscPrev.textContent = data.oscPath    || '-';
    if (aviPrev) aviPrev.textContent = data.avatarPath || '-';
}

// --- Settings: Reset Actions --------------------------------------------------
function confirmResetOsc() {
    showConfirm('Delete OSC Config?',
        'This will delete the OSC parameter config file for this avatar. VRChat will regenerate it next time the avatar is loaded.',
        () => sendMsg({ type: 'resetOscFile', avatarId: explorerAvatarId, userId: explorerUserId }));
}

function confirmResetAvatarCache() {
    showConfirm('Delete Avatar Cache?',
        'This will delete the locally cached avatar data file. All saved parameter values will be lost.',
        () => sendMsg({ type: 'resetAvatarCache', avatarId: explorerAvatarId, userId: explorerUserId }));
}

function handleResetResult(data) {
    const resultId = data.target === 'osc' ? 'osc-reset-result' : 'avi-reset-result';
    const el = document.getElementById(resultId);
    if (el) {
        el.style.display = 'block';
        el.className = 'settings-result ' + (data.success ? 'result-ok' : 'result-err');
        el.innerHTML = (data.success
            ? '<i class="fa-solid fa-circle-check"></i> '
            : '<i class="fa-solid fa-circle-exclamation"></i> ') + esc(data.message);
        setTimeout(() => { el.style.display = 'none'; }, 4000);
    }
    showToast(data.message, data.success ? 'success' : 'error');
    if (data.success) setTimeout(() => refreshData(), 600);
}

// --- Confirm Dialog ------------------------------------------------------------
let _confirmCb = null;

function showConfirm(title, msg, onConfirm) {
    _confirmCb = onConfirm;
    setEl('exp-confirm-title', title);
    setEl('exp-confirm-msg',   msg);
    const ok = document.getElementById('exp-confirm-ok');
    if (ok) ok.onclick = () => { closeConfirm(); if (_confirmCb) _confirmCb(); };
    const overlay = document.getElementById('exp-confirm-overlay');
    if (overlay) overlay.style.display = 'flex';
}

function closeConfirm() {
    const overlay = document.getElementById('exp-confirm-overlay');
    if (overlay) overlay.style.display = 'none';
    _confirmCb = null;
}

// --- Shared Helper ------------------------------------------------------------
function setEl(id, val) {
    const el = document.getElementById(id);
    if (el) el.textContent = val;
}
