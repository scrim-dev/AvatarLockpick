// --- State --------------------------------------------------------------------
let allParameters   = [];
let activeTypeFilter   = 'all';
let activeStatusFilter = 'all';
let activeQuery        = '';
let explorerAvatarId = '';
let explorerUserId   = '';

// --- Photino Bridge ----------------------------------------------------------
function sendMsg(obj) {
    if (window.external && typeof window.external.sendMessage === 'function') {
        window.external.sendMessage(JSON.stringify(obj));
    }
}

// --- Init ---------------------------------------------------------------------
window.addEventListener('load', () => {
    applyStoredTheme();
    sendMsg({ type: 'explorerReady' });

    if (window.external && typeof window.external.receiveMessage === 'function') {
        window.external.receiveMessage(raw => {
            try {
                const data = JSON.parse(raw);
                if (data.type === 'explorerData')    { handleExplorerData(data); return; }
                if (data.type === 'oscCommandResult') { handleOscResult(data);    return; }
                if (data.type === 'resetFileResult')  { handleResetResult(data);  return; }
            } catch (_) {}
        });
    }

    document.addEventListener('contextmenu', e => e.preventDefault());
    document.addEventListener('keydown', e => {
        if (e.key === 'F12' || (e.ctrlKey && e.shiftKey)) e.preventDefault();
    });
});

// --- Theme --------------------------------------------------------------------
function applyStoredTheme() {
    const themeType = localStorage.getItem('themeType');
    const root = document.documentElement;
    const defaults = {
        '--bg-color':            '#121212',
        '--bg-secondary-color':  '#181818',
        '--text-color':          '#ffffff',
        '--accent-color':        '#1E90FF',
        '--accent-hover-color':  '#46A3FF',
        '--panel-bg-color':      '#242424',
        '--border-color':        '#333333',
        '--input-bg-color':      '#2d2d2d',
        '--input-border-color':  '#454545'
    };
    const presets = {
        dodgerblue: {},
        crimson:    { '--accent-color': '#DC143C', '--accent-hover-color': '#E6395A' },
        green:      { '--accent-color': '#2ECC71', '--accent-hover-color': '#58D68D' },
        magenta:    { '--accent-color': '#DE3BFF', '--accent-hover-color': '#E566FF' }
    };
    let theme = { ...defaults };
    if (themeType === 'preset') {
        const name = localStorage.getItem('themeName');
        if (name && presets[name]) theme = { ...defaults, ...presets[name] };
    } else if (themeType === 'custom') {
        Object.keys(defaults).forEach(k => {
            const saved = localStorage.getItem('customColor_' + k);
            if (saved) theme[k] = saved;
        });
    }
    Object.keys(theme).forEach(k => root.style.setProperty(k, theme[k]));
}

// --- Data Handling ------------------------------------------------------------
function handleExplorerData(data) {
    if (data.error) {
        showError(data.error);
        return;
    }

    explorerAvatarId = data.avatarId || '';
    explorerUserId   = data.userId   || '';

    setEl('exp-tb-avatar', explorerAvatarId);

    updateStatusBar(data);

    allParameters = data.parameters || [];
    applyFilters();

    document.getElementById('exp-loading').style.display   = 'none';
    document.getElementById('exp-table-wrap').style.display = 'block';
    if (typeof renderAvatarInfo   === 'function') renderAvatarInfo(data);
    if (typeof renderSettingsPaths === 'function') renderSettingsPaths(data);
    setTimeout(showOscWarning, 900);
}

function updateStatusBar(data) {
    const oscExists = data.oscExists;
    const aviExists = data.avatarExists;

    const oscBadge = document.getElementById('sb-osc-badge');
    const aviBadge = document.getElementById('sb-avi-badge');

    oscBadge.textContent  = oscExists ? 'Found' : 'Not Found';
    oscBadge.className    = 'exp-badge ' + (oscExists ? 'exp-badge-green' : 'exp-badge-red');
    aviBadge.textContent  = aviExists ? 'Found' : 'Not Found';
    aviBadge.className    = 'exp-badge ' + (aviExists ? 'exp-badge-green' : 'exp-badge-red');

    const params = data.parameters || [];
    const matched  = params.filter(p => p.status === 'matched').length;
    const oscOnly  = params.filter(p => p.status === 'osc-only').length;
    const aviOnly  = params.filter(p => p.status === 'avatar-only').length;

    document.getElementById('stat-total').textContent   = params.length;
    document.getElementById('stat-matched').textContent = matched;
    document.getElementById('stat-osc-only').textContent = oscOnly;
    document.getElementById('stat-avi-only').textContent = aviOnly;
}

// --- Filters ------------------------------------------------------------------
function setTypeFilter(btn, val) {
    document.querySelectorAll('.exp-filter-btn[data-type]').forEach(b => b.classList.remove('active'));
    btn.classList.add('active');
    activeTypeFilter = val;
    applyFilters();
}

function setStatusFilter(btn, val) {
    document.querySelectorAll('.exp-filter-btn[data-status]').forEach(b => b.classList.remove('active'));
    btn.classList.add('active');
    activeStatusFilter = val;
    applyFilters();
}

function applyFilters() {
    const query  = activeQuery.toLowerCase();
    const tbody  = document.getElementById('exp-table-body');
    tbody.innerHTML = '';

    const filtered = allParameters.filter(p => {
        if (activeTypeFilter !== 'all'   && p.oscType.toLowerCase()  !== activeTypeFilter)   return false;
        if (activeStatusFilter !== 'all' && p.status                  !== activeStatusFilter) return false;
        if (query && !p.name.toLowerCase().includes(query) && !(p.oscInputAddress || '').toLowerCase().includes(query)) return false;
        return true;
    });

    const countEl = document.getElementById('exp-filter-count');
    countEl.textContent = filtered.length + ' / ' + allParameters.length + ' shown';

    if (filtered.length === 0) {
        document.getElementById('exp-no-results').style.display = 'block';
    } else {
        document.getElementById('exp-no-results').style.display = 'none';
        filtered.forEach(p => tbody.appendChild(buildRow(p)));
    }
}

// --- Row Builder --------------------------------------------------------------
function buildRow(p) {
    const tr = document.createElement('tr');
    tr.className = 'param-row param-row-' + p.status;

    const typeLower = (p.oscType || 'float').toLowerCase();
    const typeClass = typeLower === 'float' ? 'type-float' : typeLower === 'int' ? 'type-int' : 'type-bool';

    const savedVal = p.currentValue !== null && p.currentValue !== undefined
        ? String(p.currentValue) : '-';

    const statusMeta = getStatusMeta(p.status);

    const address = p.oscInputAddress || ('/avatar/parameters/' + p.name);

    let sendControl = '';
    if (typeLower === 'bool') {
        sendControl = `<select class="exp-value-input" id="val-${escId(p.name)}">
                          <option value="true">true (1)</option>
                          <option value="false">false (0)</option>
                       </select>`;
    } else {
        const step = typeLower === 'int' ? '1' : '0.01';
        const defVal = (p.currentValue !== null && p.currentValue !== undefined) ? p.currentValue : (typeLower === 'int' ? '0' : '0.0');
        sendControl = `<input type="number" class="exp-value-input" id="val-${escId(p.name)}" value="${defVal}" step="${step}">`;
    }

    tr.innerHTML = `
        <td class="col-name">
            <span class="param-name" title="${esc(p.name)}">${esc(p.name)}</span>
            <button class="exp-copy-btn" onclick="copyText('${esc(p.name)}')" title="Copy name"><i class="fa-solid fa-copy"></i></button>
        </td>
        <td class="col-type"><span class="exp-type-badge ${typeClass}">${esc(p.oscType)}</span></td>
        <td class="col-value"><span class="param-value">${esc(savedVal)}</span></td>
        <td class="col-status"><span class="exp-status-badge ${statusMeta.cls}">${statusMeta.icon} ${statusMeta.label}</span></td>
        <td class="col-address">
            <span class="param-address" title="${esc(address)}">${esc(address)}</span>
            <button class="exp-copy-btn" onclick="copyText('${esc(address)}')" title="Copy address"><i class="fa-solid fa-copy"></i></button>
        </td>
        <td class="col-send">${sendControl}</td>
        <td class="col-exec">
            <button class="exp-btn exp-btn-execute" onclick="executeOsc('${esc(p.name)}','${esc(address)}','${typeLower}')">
                <i class="fa-solid fa-paper-plane"></i> Send
            </button>
        </td>`;
    return tr;
}

function getStatusMeta(status) {
    switch (status) {
        case 'matched':     return { cls: 'status-matched',    icon: '<i class="fa-solid fa-circle-check"></i>',    label: 'Synced'    };
        case 'osc-only':    return { cls: 'status-osc-only',   icon: '<i class="fa-solid fa-triangle-exclamation"></i>', label: 'OSC Only'  };
        case 'avatar-only': return { cls: 'status-avatar-only',icon: '<i class="fa-solid fa-circle-question"></i>', label: 'No OSC'    };
        default:            return { cls: '',                   icon: '',                                            label: status      };
    }
}

// --- OSC Execution ------------------------------------------------------------
function executeOsc(paramName, address, oscType) {
    const inputEl = document.getElementById('val-' + escId(paramName));
    if (!inputEl) return;
    const rawValue = inputEl.value;
    sendMsg({ type: 'sendOscCommand', address, oscType, value: rawValue });
    showRowFeedback(paramName, 'sending');
}

function handleOscResult(data) {
    if (data.success) {
        showToast('UDP sent → ' + data.address, 'success');
    } else {
        showToast('Send failed: ' + data.message, 'error');
    }
}

// --- Navigation ---------------------------------------------------------------
function goBack() {
    sendMsg({ type: 'closeExplorer' });
}

function refreshData() {
    document.getElementById('exp-loading').style.display   = 'block';
    document.getElementById('exp-table-wrap').style.display = 'none';
    document.getElementById('exp-error').style.display      = 'none';
    allParameters = [];
    const aviL = document.getElementById('avi-loading');
    const aviC = document.getElementById('avi-content');
    if (aviL) aviL.style.display = 'flex';
    if (aviC) aviC.style.display = 'none';
    sendMsg({ type: 'explorerReady' });
}

// --- UI Helpers ---------------------------------------------------------------
function showError(msg) {
    document.getElementById('exp-loading').style.display = 'none';
    document.getElementById('exp-error').style.display   = 'flex';
    document.getElementById('exp-error-msg').textContent = msg;
}

function showRowFeedback(paramName, state) {
    const btn = document.querySelector(`button[onclick*="'${escId(paramName)}'"]`);
    if (!btn) return;
    if (state === 'sending') {
        btn.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> Sending';
        btn.disabled = true;
        setTimeout(() => {
            btn.innerHTML = '<i class="fa-solid fa-paper-plane"></i> Send';
            btn.disabled = false;
        }, 1500);
    }
}

function showToast(msg, type) {
    const container = document.getElementById('exp-toast-container');
    const toast = document.createElement('div');
    toast.className = 'exp-toast exp-toast-' + (type || 'info');
    const icon = type === 'success' ? 'circle-check' : type === 'error' ? 'circle-exclamation' : 'circle-info';
    toast.innerHTML = `<i class="fa-solid fa-${icon}"></i> ${esc(msg)}`;
    container.appendChild(toast);
    requestAnimationFrame(() => toast.classList.add('show'));
    setTimeout(() => {
        toast.classList.remove('show');
        setTimeout(() => toast.remove(), 350);
    }, 3000);
}

function copyText(text) {
    navigator.clipboard.writeText(text).then(() => showToast('Copied!', 'success'));
}

function esc(s) {
    return String(s)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;');
}

function escId(s) {
    return String(s).replace(/[^a-zA-Z0-9_-]/g, '_');
}

// --- Search Modal ------------------------------------------------------------
function openSearchModal() {
    document.getElementById('sm-query').value = activeQuery;
    updateModalChips('type',   activeTypeFilter);
    updateModalChips('status', activeStatusFilter);
    updateModalCount();
    document.getElementById('exp-search-overlay').classList.add('sm-open');
    setTimeout(() => document.getElementById('sm-query').focus(), 60);
    document.addEventListener('keydown', _smKeyHandler);
}

function closeSearchModal() {
    document.getElementById('exp-search-overlay').classList.remove('sm-open');
    document.removeEventListener('keydown', _smKeyHandler);
}

function _smKeyHandler(e) {
    if (e.key === 'Escape') closeSearchModal();
    if (e.key === 'Enter')  applySearchModal();
}

function applySearchModal() {
    activeQuery        = (document.getElementById('sm-query').value || '').trim();
    const tc = document.querySelector('#sm-type-chips   .sm-chip.active');
    const sc = document.querySelector('#sm-status-chips .sm-chip.active');
    activeTypeFilter   = tc ? tc.dataset.val : 'all';
    activeStatusFilter = sc ? sc.dataset.val : 'all';
    applyFilters();
    updateSearchBtn();
    closeSearchModal();
}

function clearSearchModal() {
    activeQuery        = '';
    activeTypeFilter   = 'all';
    activeStatusFilter = 'all';
    document.getElementById('sm-query').value = '';
    updateModalChips('type',   'all');
    updateModalChips('status', 'all');
    updateModalCount();
    applyFilters();
    updateSearchBtn();
    closeSearchModal();
}

function presetSearch(term) {
    document.getElementById('sm-query').value = term;
    updateModalCount();
}

function setModalChip(group, el) {
    document.querySelectorAll('#sm-' + group + '-chips .sm-chip').forEach(c => c.classList.remove('active'));
    el.classList.add('active');
    updateModalCount();
}

function updateModalChips(group, val) {
    document.querySelectorAll('#sm-' + group + '-chips .sm-chip').forEach(c => {
        c.classList.toggle('active', c.dataset.val === val);
    });
}

function updateModalCount() {
    const query  = (document.getElementById('sm-query').value || '').toLowerCase().trim();
    const tc     = document.querySelector('#sm-type-chips   .sm-chip.active');
    const sc     = document.querySelector('#sm-status-chips .sm-chip.active');
    const type   = tc ? tc.dataset.val : 'all';
    const status = sc ? sc.dataset.val : 'all';
    const count  = allParameters.filter(p => {
        if (type   !== 'all' && p.oscType.toLowerCase() !== type)   return false;
        if (status !== 'all' && p.status                !== status) return false;
        if (query  && !p.name.toLowerCase().includes(query) && !(p.oscInputAddress || '').toLowerCase().includes(query)) return false;
        return true;
    }).length;
    const el = document.getElementById('sm-result-count');
    if (el) el.textContent = count + ' of ' + allParameters.length + ' parameters match';
}

function showOscWarning() {
    const el = document.getElementById('exp-osc-warning');
    if (el) el.classList.add('osc-warn-visible');
}

function dismissOscWarning() {
    const el = document.getElementById('exp-osc-warning');
    if (el) el.classList.remove('osc-warn-visible');
}

function updateSearchBtn() {
    const hasFilter = activeQuery || activeTypeFilter !== 'all' || activeStatusFilter !== 'all';
    const dot = document.getElementById('exp-search-active-dot');
    const btn = document.getElementById('exp-search-btn');
    const sum = document.getElementById('exp-filter-summary');
    if (dot) dot.style.display = hasFilter ? 'inline-block' : 'none';
    if (btn) btn.classList.toggle('active-filter', hasFilter);
    if (sum) {
        if (!hasFilter) { sum.innerHTML = ''; return; }
        const parts = [];
        if (activeQuery)                  parts.push('<span class="filter-tag">' + esc(activeQuery) + '</span>');
        if (activeTypeFilter   !== 'all') parts.push('<span class="filter-tag">' + activeTypeFilter   + '</span>');
        if (activeStatusFilter !== 'all') parts.push('<span class="filter-tag">' + activeStatusFilter + '</span>');
        sum.innerHTML = parts.join('');
    }
}
