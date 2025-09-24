<%@ Page Language="C#" AutoEventWireup="true" Async="true" CodeBehind="Admin_TemplateEditorInterface.aspx.cs" Inherits="fyp.OMTS_Pages.OMTS_Admin.Admin_TemplateEditorInterface" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
  <meta charset="UTF-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1.0"/>
  <title>A4 Template Designer</title>
  <style>
    *{margin:0;padding:0;box-sizing:border-box}
    html,body{height:100%}
    body{font-family:'Segoe UI',Tahoma,Geneva,Verdana,sans-serif;background:#2b2b2b;color:#fff;overflow:hidden}

    /* Layout */
    .app-root{display:flex;flex-direction:column;height:100%}
    .topbar{
      display:flex;align-items:flex-start;gap:12px;
      background:#1e1e1e;border-bottom:1px solid #404040;padding:8px 10px;overflow-x:auto;
      position: sticky; top: 0; z-index: 1000;
    }
    .topbar .btn{
      min-width:64px;min-height:64px;background:#404040;border:1px solid #555;border-radius:10px;
      color:#fff;cursor:pointer;display:flex;flex-direction:column;align-items:center;justify-content:center;
      font-size:18px;user-select:none;padding:6px 8px;transition:transform .12s, background .12s
    }
    .topbar .btn span{font-size:11px;color:#cfcfcf;margin-top:4px}
    .topbar .btn:hover{background:#4b4b4b;transform:translateY(-1px)}
    .topbar .btn.active{background:#0078d4;border-color:#0078d4}
    .topbar .btn:disabled{opacity:.55;cursor:not-allowed}

    .main{flex:1;display:flex;min-height:0;position:relative;}

    /* Sidebars */
    .left-panel{width:350px;background:#1e1e1e;border-right:1px solid #404040;padding:16px;overflow-y:auto;z-index:2;position:relative;height:100%;display:flex;flex-direction:column;min-height:100vh;}
    .right-panel{width:280px;background:#1e1e1e;border-left:1px solid #404040;padding:16px;overflow:auto;z-index:2;position:relative;}

    /* Properties */
    .property-group{margin-bottom:16px}
    .property-label{display:block;margin-bottom:8px;font-size:12px;color:#ccc;text-transform:uppercase;font-weight:600}
    .property-row{display:flex;gap:8px;align-items:center}
    .property-input{flex:1;min-width:0;padding:8px 10px;background:#404040;border:1px solid #555;border-radius:6px;color:#fff;font-size:14px}
    .property-input:focus{border-color:#0078d4;outline:none}
    .color-row{display:flex;align-items:center;gap:8px}
    .color-input{width:44px;height:40px;border:none;border-radius:6px;cursor:pointer;padding:0}
    .tag-btn{padding:8px 10px;background:#2b2b2b;border:1px solid #555;border-radius:6px;color:#ddd;cursor:pointer;font-size:12px}
    .tag-btn.active{background:#006b5b;border-color:#22c3a6}
    .slider{width:100%;height:6px;background:#404040;border-radius:3px;outline:none;-webkit-appearance:none}
    .slider::-webkit-slider-thumb{-webkit-appearance:none;width:16px;height:16px;background:#0078d4;border-radius:50%;cursor:pointer}

    /* Center work area */
    .center{flex:1;display:flex;align-items:center;justify-content:center;position:relative;background:#333;min-width:0;z-index:1;overflow:hidden;}
    .canvas-container{position:relative;background:#fff;box-shadow:0 10px 30px rgba(0,0,0,.5);border-radius:8px;transform:translate(0,0) scale(1);transform-origin: center center;overflow:visible;will-change:transform;}
    .canvas{width:794px;height:1123px;position:relative;background:#fff;border-radius:8px;overflow:visible;}
    .grid{position:absolute;inset:0;opacity:.1;background-image:
      linear-gradient(to right,#000 1px,transparent 1px),linear-gradient(to bottom,#000 1px,transparent 1px);
      background-size:20px 20px;pointer-events:none}

    /* Elements */
    /* Drag/move handle (top-center "move" puck) */
    .drag-handle{
      position:absolute;
      top:-16px;                 /* slightly above the box */
      left:50%;
      transform:translateX(-50%);
      width:22px;
      height:22px;
      border-radius:50%;
      background:rgba(0,0,0,0.7);
      color:#fff;
      display:none;
      align-items:center;
      justify-content:center;
      font-size:12px;
      line-height:1;
      border:1px solid #888;
      cursor:grab;
      user-select:none;
      opacity:0.9;
    }
    .draggable-element.selected .drag-handle{display:flex}
    .drag-handle:active{ cursor:grabbing; }
    .draggable-element:hover .drag-handle{ opacity:1; }

    /* Optional: a small stem so it visually "connects" to the box */
    .drag-handle::after{
      content:"";
      position:absolute;
      bottom:-6px;
      left:50%;
      transform:translateX(-50%);
      width:1px;
      height:6px;
      background:#888;
      opacity:0.7;
    }


    .draggable-element{position:absolute;cursor:move;border:2px solid transparent;min-width:50px;min-height:20px;user-select:none}
    .draggable-element.selected{border-color:#0078d4;box-shadow:0 0 0 1px rgba(0,120,212,.25)}
    .draggable-element[data-locked="true"].selected{border-color:#ffb100;box-shadow:0 0 0 1px rgba(255,177,0,.35)}
    .resize-handle{position:absolute;width:9px;height:9px;background:#0078d4;border:1px solid #fff;border-radius:50%;display:none}
    .draggable-element.selected .resize-handle{display:block}
    .resize-handle.nw{top:-5px;left:-5px;cursor:nw-resize}
    .resize-handle.ne{top:-5px;right:-5px;cursor:ne-resize}
    .resize-handle.sw{bottom:-5px;left:-5px;cursor:sw-resize}
    .resize-handle.se{bottom:-5px;right:-5px;cursor:se-resize}

    .text-element{position:absolute;inset:0;background:transparent;color:#000;font-family:Arial,sans-serif;
      font-size:16px;padding:6px;border:none;outline:none;resize:none;overflow:hidden}
    .image-element{background-size:cover;background-position:center;background-repeat:no-repeat;border-radius:4px;
      position: absolute;
      overflow: hidden;       /* optional, keeps things tidy when resizing smaller */
      border-radius: 4px;
    }

    /* The actual image; never crop, preserve aspect ratio */
    .image-element .image-content{
      width: 100%;
      height: 100%;
      object-fit: contain;    /* no crop; letterbox if box aspect differs */
      pointer-events: none;   /* clicks go to the draggable container / handles */
      user-select: none;
    }
    /* Data placeholder behaves like text but with dashed look on container */
    .data-element{border:2px dashed #ccc}

    /* Layers */
    .layers-title{margin-bottom:10px;color:#ccc}
    .layer-item{display:flex;align-items:center;gap:10px;padding:8px;background:#2b2b2b;margin-bottom:6px;border-radius:6px;cursor:grab;transition:all 0.2s ease}
    .layer-item.selected{background:#0078d4;box-shadow:0 0 0 2px #0078d4, 0 0 8px rgba(0,120,212,0.4);border:1px solid #0078d4}
    .layer-visibility,.layer-lock{width:18px;height:18px;cursor:pointer;user-select:none}
    .layer-name{flex:1;white-space:nowrap;overflow:hidden;text-overflow:ellipsis}
    .layer-item.dragging{opacity:.6;outline:1px dashed #999}

    /* Context menu */
    .context-menu{position:absolute;background:#1e1e1e;border:1px solid #404040;border-radius:8px;padding:6px 0;
      box-shadow:0 8px 24px rgba(0,0,0,.4);z-index:1000;display:none;min-width:160px;max-width:200px}
    .context-menu-item{padding:10px 16px;cursor:pointer;font-size:13px;color:#ffffff;border:none;background:none;width:100%;text-align:left;display:block;transition:background-color 0.15s ease}
    .context-menu-item:hover{background:#404040;color:#ffffff}
    .context-menu-item:active{background:#0078d4;color:#ffffff}

    /* Zoom HUD */
    /* Add this to your existing styles */
    .zoom-controls {
      position: fixed;
      bottom: 16px;
      right: 16px;
      display: flex !important; /* Force display */
      gap: 8px;
      align-items: center;
      background: rgba(0, 0, 0, .85);
      padding: 10px;
      border-radius: 8px;
      z-index: 9999; /* Ensure it's above everything */
      visibility: visible !important;
      opacity: 1 !important;
    }

    .zoom-btn {
      min-width: 32px;
      height: 32px;
      background: #404040;
      border: 1px solid #555;
      border-radius: 6px;
      color: #fff;
      cursor: pointer;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 16px;
      font-weight: bold;
    }
    
    .zoom-btn:hover {
      background: #4b4b4b;
    }
    
    .zoom-level {
      color: #fff;
      font-size: 14px;
      min-width: 60px;
      text-align: center;
      font-weight: 500;
    }

    /* Modal and Popup Styles */
    .modal-overlay{position:fixed;top:0;left:0;right:0;bottom:0;background:rgba(0,0,0,0.7);z-index:10000;display:flex;align-items:center;justify-content:center;backdrop-filter:blur(4px)}
    .modal{background:#1e1e1e;border:1px solid #404040;border-radius:12px;padding:24px;max-width:500px;width:90%;box-shadow:0 20px 40px rgba(0,0,0,0.5);animation:modalSlideIn 0.3s ease-out}
    .modal h3{margin:0 0 16px 0;color:#fff;font-size:18px;font-weight:600}
    .modal p{margin:0 0 20px 0;color:#ccc;line-height:1.5}
    .modal-buttons{display:flex;gap:12px;justify-content:flex-end;margin-top:24px}
    .modal-btn{padding:10px 20px;border:none;border-radius:6px;cursor:pointer;font-size:14px;font-weight:500;transition:all 0.2s ease}
    .modal-btn-primary{background:#0078d4;color:#fff}
    .modal-btn-primary:hover{background:#106ebe;transform:translateY(-1px)}
    .modal-btn-secondary{background:#404040;color:#fff;border:1px solid #555}
    .modal-btn-secondary:hover{background:#4b4b4b;transform:translateY(-1px)}
    .modal-btn-danger{background:#d13438;color:#fff}
    .modal-btn-danger:hover{background:#b71c1c;transform:translateY(-1px)}
    .modal input[type="text"],.modal input[type="url"]{width:100%;padding:12px;background:#2b2b2b;border:1px solid #555;border-radius:6px;color:#fff;font-size:14px;margin:8px 0}
    .modal input[type="text"]:focus,.modal input[type="url"]:focus{outline:none;border-color:#0078d4;box-shadow:0 0 0 2px rgba(0,120,212,0.2)}
    @keyframes modalSlideIn{from{opacity:0;transform:scale(0.9) translateY(-20px)}to{opacity:1;transform:scale(1) translateY(0)}}

    /* Success Message */
    .success-message{position:fixed;top:20px;right:20px;background:#107c10;color:#fff;padding:16px 20px;border-radius:8px;box-shadow:0 4px 12px rgba(0,0,0,0.3);z-index:10001;animation:successSlideIn 0.3s ease-out;max-width:300px}
    .success-message::before{content:"✓";margin-right:8px;font-weight:bold;font-size:16px}
    @keyframes successSlideIn{from{opacity:0;transform:translateX(100%)}to{opacity:1;transform:translateX(0)}}
  </style>
</head>
<body>
  <form id="form1" runat="server">
    <!-- Hidden fields for server-side data -->
    <asp:HiddenField ID="hdnTemplateData" runat="server" />
    <asp:HiddenField ID="hdnTemplateName" runat="server" />
    <asp:HiddenField ID="hdnTemplateDescription" runat="server" />
    <asp:HiddenField ID="hdnTemplateType" runat="server" />
    <asp:HiddenField ID="hdnTemplateImageUrl" runat="server" />
    <asp:HiddenField ID="hdnAction" runat="server" Value="create" />

  <div class="app-root">
    <!-- TOPBAR -->
    <div class="topbar" id="topbar">
      <!-- Tools -->
      <button type="button" class="btn active" data-tool="select" title="Select (V)"><div>🖱️</div><span>Select</span></button>
      <button type="button" class="btn" data-tool="pan" title="Pan / Drag view (H)"><div>✋</div><span>Pan</span></button>
      <button type="button" class="btn" data-tool="text" title="Add Text (T)"><div>T</div><span>Text</span></button>
      <button type="button" class="btn" data-tool="image" title="Add Image (I)"><div>🖼️</div><span>Image</span></button>
      <button type="button" class="btn" data-tool="placeholder" title="Add Data Placeholder (P)"><div>{{ }}</div><span>Data</span></button>
      <button type="button" class="btn" data-tool="rectangle" title="Add Rectangle (R)"><div>⬜</div><span>Rect</span></button>
      <button type="button" class="btn" data-tool="circle" title="Add Circle (O)"><div>⭕</div><span>Circle</span></button>

      <!-- Actions -->
      <button type="button" class="btn" id="btnUndo" title="Undo (Ctrl+Z)" onclick="undo()"><div>↶</div><span>Undo</span></button>
      <button type="button" class="btn" id="btnRedo" title="Redo (Ctrl+Y)" onclick="redo()"><div>↷</div><span>Redo</span></button>
      <button type="button" class="btn" title="Save Template" onclick="saveTemplate()"><div>💾</div><span>Save</span></button>
      <button type="button" class="btn" title="Save to Server" onclick="saveTemplateToServer()"><div>☁️</div><span>Save to Server</span></button>
      <button type="button" class="btn" title="Load Template" onclick="loadTemplate()"><div>📥</div><span>Load</span></button>
      <button type="button" class="btn" title="Close" onclick="showCloseWarning()" style="background:#d13438;border-color:#d13438;margin-left:12px;"><div>✕</div><span>Close</span></button>
    </div>

    <div class="main">
      <!-- LEFT: Properties -->
      <div class="left-panel">
        <h3 style="margin-bottom:10px;">Properties</h3>
        <p id="propertiesTip" style="color:#ccc;font-size:14px;margin-bottom:16px;font-style:italic;font-weight:500;">Click on a layer or object to view its properties.</p>

        <div id="propertiesContent" style="display:none;flex:1;overflow-y:auto;max-height:calc(108vh - 200px);">
          <div class="property-group">
            <label class="property-label">Position</label>
            <div class="property-row">
              <input type="number" class="property-input" id="posX" placeholder="X" onchange="updateSelectedElement(true)">
              <input type="number" class="property-input" id="posY" placeholder="Y" onchange="updateSelectedElement(true)">
            </div>
          </div>

          <div class="property-group">
            <label class="property-label">Size</label>
            <div class="property-row">
              <input type="number" class="property-input" id="width" placeholder="Width" onchange="updateSelectedElement(true)">
              <input type="number" class="property-input" id="height" placeholder="Height" onchange="updateSelectedElement(true)">
            </div>
          </div>

          <div class="property-group" id="textProperties" style="display:none;">
            <label class="property-label">Text Style</label>
            <div class="property-row">
              <select class="property-input" id="fontFamily" onchange="updateSelectedElement(true)">
                <option value="Arial, sans-serif">Arial</option>
                <option value="Times New Roman, serif">Times New Roman</option>
                <option value="Helvetica, sans-serif">Helvetica</option>
                <option value="Georgia, serif">Georgia</option>
                <option value="Verdana, sans-serif">Verdana</option>
                <option value="Courier New, monospace">Courier New</option>
              </select>
            </div>
            <div class="property-row" style="margin-top:8px;">
              <input type="range" class="slider" id="fontSize" min="8" max="96" value="16" oninput="syncFontSizeFromSlider()" onchange="updateSelectedElement(true)" style="width:75%;">
              <input type="number" class="property-input" id="fontSizeNumber" min="8" max="300" value="16" style="width:25%;" oninput="syncFontSizeFromNumber()" onchange="updateSelectedElement(true)">
            </div>
            <div class="property-row" style="margin-top:8px;">
              <select class="property-input" id="fontWeight" onchange="updateSelectedElement(true)">
                <option value="normal">Normal</option>
                <option value="bold">Bold</option>
                <option value="lighter">Light</option>
              </select>
              <select class="property-input" id="textAlign" onchange="updateSelectedElement(true)">
                <option value="left">Left</option>
                <option value="center">Center</option>
                <option value="right">Right</option>
                <option value="justify">Justify</option>
              </select>
            </div>
            <div class="color-row" style="margin-top:8px;">
              <label class="property-label" style="margin:0;">Text Color</label>
              <input type="color" class="color-input" id="textColor" value="#000000" onchange="updateSelectedElement(true)">
              <button type="button" class="tag-btn" id="textTransparent" onclick="toggleTextTransparent(event)">Transparent</button>
            </div>
          </div>

          <div class="property-group" id="placeholderProperties" style="display:none;">
              <label class="property-label">Data Field</label>
              <div class="property-row">
                <select class="property-input" id="dataFieldDdl" onchange="onPlaceholderFieldChange()">
                  <option value="">-- Select --</option>
                  <option value="name">Name</option>
                  <option value="venue">Venue</option>
                  <option value="date">Date</option>
                  <option value="signature">Signature</option>
                  <option value="eventName">Event Name</option>
                  <option value="achievement">Achievement</option>
                  <option value="recognition">Recognition</option>
                  <option value="other">Other...</option>
                </select>
              </div>
              <div class="property-row" id="customFieldRow" style="display:none;margin-top:8px;">
                <input type="text" class="property-input" id="dataFieldCustom" placeholder="Enter custom field name" oninput="onPlaceholderFieldChange()">
              </div>
              <label class="property-label" style="margin-top:12px;">Default Value</label>
              <input type="text" class="property-input" id="defaultValue" placeholder="Default text" onchange="updateSelectedElement(true)">
            </div>

          <div class="property-group">
            <label class="property-label">Background</label>
            <div class="color-row">
              <input type="color" class="color-input" id="backgroundColor" value="#ffffff" onchange="updateSelectedElement(true)">
              <button type="button" class="tag-btn" id="bgTransparent" onclick="toggleBgTransparent(event)">Transparent</button>
            </div>

            <label class="property-label" style="margin-top:12px;">Border</label>
            <div class="property-row">
              <input type="number" class="property-input" id="borderWidth" placeholder="Width" onchange="updateSelectedElement(true)">
              <input type="color" class="color-input" id="borderColor" value="#000000" onchange="updateSelectedElement(true)">
              <button type="button" class="tag-btn" id="borderTransparent" onclick="toggleBorderTransparent(event)">Transparent</button>
            </div>
          </div>

          <div class="property-group">
            <label class="property-label">Opacity</label>
            <input type="range" class="slider" id="opacity" min="0" max="100" value="100" onchange="updateSelectedElement(true)">
            <div style="margin-top:6px;color:#aaa;font-size:12px;">Opacity: <span id="opacityValue">100%</span></div>
          </div>
        </div>

        <!-- Scroll tip at bottom left -->
        <div id="scrollTip" style="position:sticky;bottom:0;left:0;right:0;color:#888;font-size:11px;font-style:italic;margin-top:16px;padding:8px;background:rgba(0,0,0,0.3);border-radius:4px;">
          <div>Tip: Use mouse wheel or touchpad to scroll, or use pan tool to navigate the canvas</div>
        </div>
      </div>

      <!-- CENTER: Canvas -->
      <div class="center">
        <div class="canvas-container" id="canvasContainer">
          <div class="canvas" id="canvas">
            <div class="grid"></div>
          </div>
        </div>
      </div>

      <!-- RIGHT: Layers -->
      <div class="right-panel">
        <h3 class="layers-title">Layers (top → bottom)</h3>
        <div id="layersList" onclick="handleLayersPanelClick(event)"></div>
        <div class="zoom-controls">
          <button type="button" class="zoom-btn" onclick="adjustZoom(-0.1)">−</button>
          <button type="button" class="zoom-btn" onclick="zoomToFit()">Fit</button>
          <div class="zoom-level" id="zoomLevel">100%</div>
          <button type="button" class="zoom-btn" onclick="adjustZoom(0.1)">+</button>
        </div>
      </div>
    </div>
  </div>

  <!-- Context Menu -->
  <div class="context-menu" id="contextMenu">
    <button type="button" class="context-menu-item" id="ctx-duplicate" onclick="duplicateElement()">Duplicate</button>
    <button type="button" class="context-menu-item" id="ctx-delete" onclick="deleteElement()">Delete</button>
    <button type="button" class="context-menu-item" id="ctx-moveUp" onclick="moveLayerUp()" style="display:none">Move Up</button>
    <button type="button" class="context-menu-item" id="ctx-moveDown" onclick="moveLayerDown()" style="display:none">Move Down</button>
    <button type="button" class="context-menu-item" id="ctx-bringFront" onclick="bringToFront()">Bring to Front</button>
    <button type="button" class="context-menu-item" id="ctx-sendBack" onclick="sendToBack()">Send to Back</button>
    <button type="button" class="context-menu-item" id="ctx-rename" onclick="renameElement()" style="display:none">Rename</button>
  </div>

  <!-- File Inputs -->
  <input type="file" class="file-input" id="imageUpload" accept="image/*" onchange="handleImageUpload(event)">
  <input type="file" class="file-input" id="templateUpload" accept=".json" onchange="handleTemplateUpload(event)">

  <!-- Firebase SDKs (Compat build for quick drop-in) -->
    <script src="https://www.gstatic.com/firebasejs/9.23.0/firebase-app-compat.js"></script>
    <script src="https://www.gstatic.com/firebasejs/9.23.0/firebase-database-compat.js"></script>
    <script src="https://www.gstatic.com/firebasejs/9.23.0/firebase-auth-compat.js"></script>
    <script>
        // 1) Replace these with your actual config values
        const firebaseConfig = {
            apiKey: "AIzaSyD7OIF-dnrsre_YaDVuWjw527whdmaSoi0",
            authDomain: "pgy-omts.firebaseapp.com",
            databaseURL: "https://pgy-omts-default-rtdb.asia-southeast1.firebasedatabase.app",
            projectId: "pgy-omts",
            storageBucket: "pgy-omts.firebasestorage.app",
            messagingSenderId: "388216837936",
            appId: "1:388216837936:web:bd346048b18f5313fc5af5",
            measurementId: "G-BMBETQNEMC"
        };

        // 2) Init
        firebase.initializeApp(firebaseConfig);
        const db = firebase.database();

        // 3) (Optional) Anonymous auth — helps if your rules require signed-in users
        firebase.auth().signInAnonymously().catch(console.error);
    </script>
    <script>
        /* ===== Globals ===== */
        let currentTool = 'select';      // 'select' | 'pan' | 'text' | 'image' | 'placeholder' | 'rectangle' | 'circle'
        let selectedElement = null;
        let isDragging = false;
        let dragOffset = { x: 0, y: 0 }; // in canvas coords
        let elementCounter = 0;
        let zoomLevel = 1.0;
        let elements = [];               // {id,type,element}
        let panOffset = { x: 0, y: 0 };

        // history (undo/redo)
        let history = [];
        let historyIndex = -1;
        const HISTORY_LIMIT = 100;
        let historyHold = false;

        // zoom constants
        const MIN_ZOOM = 0.2;
        const MAX_ZOOM = 2.5;
        const FIT_PADDING = { top: 20, right: 20, bottom: 20, left: 20 };

        // snap & guides
        let snapEnabled = true;
        const SNAP_PX = 6;
        let guideH = null, guideV = null;

        /* ===== Init ===== */
        document.addEventListener('DOMContentLoaded', () => {
            setupUI();
            setupEventListeners();
            updateLayersList();
            requestAnimationFrame(() => { zoomToFit(); pushHistory(true); });
        });
        window.addEventListener('resize', () => zoomToFit());

        function setupUI() {
            document.getElementById('topbar').addEventListener('click', e => {
                const b = e.target.closest('.btn');
                if (!b) return;
                if (b.dataset.tool) {
                    // Special handling for image tool
                    if (b.dataset.tool === 'image') {
                        setActiveTool('image');
                        showImageUrlModal();
                        return;
                    }
                    setActiveTool(b.dataset.tool);
                }
            });
            setUndoRedoState();

            // Force show zoom controls on load
            setTimeout(() => {
                const zoomControls = document.querySelector('.zoom-controls');
                if (zoomControls) {
                    zoomControls.style.display = 'flex';
                    zoomControls.style.visibility = 'visible';
                    zoomControls.style.opacity = '1';
                }
            }, 100);
        }
        function setActiveTool(tool) {
            currentTool = tool;
            document.querySelectorAll('.topbar .btn').forEach(btn => {
                if (btn.dataset.tool) btn.classList.toggle('active', btn.dataset.tool === tool);
            });
        }

        function setupEventListeners() {
            const canvas = document.getElementById('canvas');

            // Pointer events
            canvas.addEventListener('pointerdown', handlePointerDown);
            canvas.addEventListener('pointermove', handlePointerMove);
            canvas.addEventListener('pointerup', handlePointerUp);

            // Mouse events for drag handle compatibility
            canvas.addEventListener('mousedown', handlePointerDown);
            canvas.addEventListener('mousemove', handlePointerMove);
            canvas.addEventListener('mouseup', handlePointerUp);

            // Context menu
            canvas.addEventListener('contextmenu', handleContextMenu);
            document.addEventListener('click', hideContextMenu);

            // Allow deselecting by clicking outside the canvas area
            document.addEventListener('click', function (e) {
                if (!e.target.closest('.canvas') && !e.target.closest('.left-panel') && !e.target.closest('.right-panel') && !e.target.closest('.topbar')) {
                    if (selectedElement) {
                        selectedElement.classList.remove('selected');
                        selectedElement = null;
                        updatePropertiesPanel();
                        updateLayersList(); // Update layers to remove highlight
                    }
                }
            });

            // Keyboard
            document.addEventListener('keydown', handleKeyDown);

            // live labels
            document.getElementById('opacity').addEventListener('input', function () {
                document.getElementById('opacityValue').textContent = this.value + '%';
            });

            // Space pan + Hand tool + wheel zoom
            const canvasArea = document.querySelector('.center');
            let spacePanning = false;
            let panStart = { x: 0, y: 0 };

            document.addEventListener('keydown', e => {
                if (e.code === 'Space') { spacePanning = true; document.body.style.cursor = 'grab'; }
                if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 'z') { e.preventDefault(); undo(); }
                if ((e.ctrlKey || e.metaKey) && (e.key.toLowerCase() === 'y' || (e.shiftKey && e.key.toLowerCase() === 'z'))) { e.preventDefault(); redo(); }
                if (e.key === 'v' || e.key === 'V') setActiveTool('select');
                if (e.key === 'h' || e.key === 'H') setActiveTool('pan');
                if (e.key === 't' || e.key === 'T') setActiveTool('text');
                if (e.key === 'i' || e.key === 'I') setActiveTool('image');
                if (e.key === 'p' || e.key === 'P') setActiveTool('placeholder');
                if (e.key === 'r' || e.key === 'R') setActiveTool('rectangle');
                if (e.key === 'o' || e.key === 'O') setActiveTool('circle');
                if ((e.ctrlKey || e.metaKey) && e.key === '0') { e.preventDefault(); zoomToFit(); }
            });
            document.addEventListener('keyup', e => { if (e.code === 'Space') { spacePanning = false; document.body.style.cursor = ''; } });

            canvasArea.addEventListener('pointerdown', e => {
                if (!(spacePanning || currentTool === 'pan')) return;
                panStart = { x: e.clientX - panOffset.x, y: e.clientY - panOffset.y };
                document.body.style.cursor = 'grabbing';
                const move = ev => { panOffset = { x: ev.clientX - panStart.x, y: ev.clientY - panStart.y }; clampPan(); updateZoomTransform(); };
                const up = () => { document.removeEventListener('pointermove', move); document.removeEventListener('pointerup', up); document.body.style.cursor = ''; };
                document.addEventListener('pointermove', move);
                document.addEventListener('pointerup', up);
            });

            // Cursor-anchored pinch/scroll zoom (Ctrl+wheel)
            canvasArea.addEventListener('wheel', e => {
                if (!e.ctrlKey) return;
                e.preventDefault();
                const factor = Math.exp(-e.deltaY * 0.0015);
                const newZoom = zoomLevel * factor;
                zoomTo(newZoom, { x: e.clientX, y: e.clientY });
            }, { passive: false });

            // Mouse wheel panning (both directions)
            canvasArea.addEventListener('wheel', function (e) {
                if (!e.ctrlKey) { // Only pan if not zooming
                    e.preventDefault();
                    // Typical: vertical scroll pans Y, horizontal scroll pans X
                    panOffset.x -= e.deltaX;
                    panOffset.y -= e.deltaY;
                    clampPan();
                    updateZoomTransform();
                }
            }, { passive: false });
        }

        function onPlaceholderFieldChange() {
            const ddl = document.getElementById('dataFieldDdl');
            const customRow = document.getElementById('customFieldRow');
            const customInput = document.getElementById('dataFieldCustom');

            if (ddl.value === 'other') {
                customRow.style.display = 'block';
                customInput.focus();
            } else {
                customRow.style.display = 'none';
                customInput.value = '';
            }

            // Update the selected element
            updateSelectedElement(true);
        }

        /* ===== Font size bidirectional sync ===== */
        function syncFontSizeFromSlider() {
            document.getElementById('fontSizeNumber').value = document.getElementById('fontSize').value;
        }
        function syncFontSizeFromNumber() {
            const v = document.getElementById('fontSizeNumber').value;
            document.getElementById('fontSize').value = v;
        }

        // Normalize Google Drive share links to a direct-view URL
        // Grab Google Drive file ID from any of the common link shapes.
        function extractDriveFileId(input) {
            if (!input) return null;
            const trimmed = input.trim();
            // Accept a bare ID too
            if (/^[A-Za-z0-9_\-]{20,}$/.test(trimmed)) return trimmed;
            try {
                const u = new URL(trimmed);
                // /file/d/<id>/...
                const m = u.pathname.match(/\/file\/d\/([^/]+)/);
                if (m && m[1]) return m[1];
                // ?id=<id>
                const qid = u.searchParams.get('id');
                if (qid) return qid;
            } catch (_) { }
            return null;
        }

        // Try to produce a direct image URL that actually loads.
        // Prefers drive.usercontent.google.com (what Drive redirects to)
        // and falls back to the uc?id= form if needed.
        async function toDriveDirectUrl(input) {
            const id = extractDriveFileId(input);
            if (!id) return input; // not a Drive link/ID, use as-is

            const candidates = [
                // Preferred: usercontent download endpoint
                //`https://drive.usercontent.google.com/download?id=${id}&export=download`,
                //// Fallback: uc view endpoint
                //`https://drive.google.com/uc?id=${id}&export=view`,
                // Thumbnail proxy (often works best)
                `https://drive.google.com/thumbnail?id=${id}&sz=w2000`
            ];

            for (const url of candidates) {
                if (await canLoadImage(url)) {
                    console.log(url);
                    return url;
                }
            }
            // If all fail, return original so user sees something is wrong
            return input;
        }
        // Lightweight check: will the browser load it as an <img>?
        function canLoadImage(url) {
            return new Promise(resolve => {
                const img = new Image();
                img.onload = () => resolve(true);
                img.onerror = () => resolve(false);
                img.referrerPolicy = 'no-referrer'; // helps with some hosts
                img.src = url;
            });
        }


        // Create an image element from a public URL (centered)
        async function createImageFromUrl(rawUrl) {
            // If you have Drive conversion, keep using your toDriveDirectUrl()
            const url = (typeof toDriveDirectUrl === 'function') ? await toDriveDirectUrl(rawUrl) : rawUrl;

            // Load the image to get intrinsic size
            const probe = await loadImage(url); // returns {img, w, h}

            // Compute a reasonable initial box size (fit within 80% of canvas)
            const CANVAS_W = 794, CANVAS_H = 1123;
            const MAX_W = Math.floor(CANVAS_W * 0.8);
            const MAX_H = Math.floor(CANVAS_H * 0.8);
            const sized = fitWithin(probe.w, probe.h, MAX_W, MAX_H); // {w,h}

            // Center on the page
            const cx = CANVAS_W / 2, cy = CANVAS_H / 2;
            const x = Math.round(cx - sized.w / 2);
            const y = Math.round(cy - sized.h / 2);

            // Build element with an <img>
            const el = document.createElement('div');
            el.className = 'draggable-element image-element';
            wrapCommon(el, x, y, sized.w, sized.h, 'image');

            const img = document.createElement('img');
            img.className = 'image-content';
            img.src = url;
            el.appendChild(img);

            // Keep URL for save/load
            el.dataset.imageUrl = url;

            if (typeof attachDragHandle === 'function') attachDragHandle(el);

            // Auto-switch back to select tool after creating image
            setActiveTool('select');

            pushHistory();
        }

        // helpers
        function loadImage(url) {
            return new Promise((resolve, reject) => {
                const img = new Image();
                img.onload = () => resolve({ img, w: img.naturalWidth, h: img.naturalHeight });
                img.onerror = reject;
                img.referrerPolicy = 'no-referrer'; // helps with some hosts
                img.src = url;
            });
        }
        function fitWithin(w, h, maxW, maxH) {
            const scale = Math.min(maxW / w, maxH / h, 1); // don't scale up small images
            return { w: Math.max(1, Math.round(w * scale)), h: Math.max(1, Math.round(h * scale)) };
        }

        /* ===== Coordinate helpers ===== */
        function clientToCanvas(ev) {
            const cont = document.getElementById('canvasContainer').getBoundingClientRect();
            const x = (ev.clientX - cont.left - panOffset.x) / zoomLevel;
            const y = (ev.clientY - cont.top - panOffset.y) / zoomLevel;
            return { x, y };
        }

        /* ===== Pan clamp ===== */
        function clampPan() {
            // Allow free panning in all directions (no clamping)
            // panOffset.x and panOffset.y are not restricted
        }

        /* ===== Zoom helpers ===== */
        function getDocumentCenterClient() {
            const rect = document.getElementById('canvasContainer').getBoundingClientRect();
            return { x: rect.left + rect.width / 2, y: rect.top + rect.height / 2 };
        }
        function zoomTo(newZoom, anchorClient) {
            const contRect = document.getElementById('canvasContainer').getBoundingClientRect();
            newZoom = Math.max(MIN_ZOOM, Math.min(MAX_ZOOM, newZoom));

            const ax = anchorClient.x - contRect.left;
            const ay = anchorClient.y - contRect.top;

            const oldZ = zoomLevel;
            const axCanvas = (ax - panOffset.x) / oldZ;
            const ayCanvas = (ay - panOffset.y) / oldZ;

            zoomLevel = newZoom;
            panOffset.x = ax - axCanvas * zoomLevel;
            panOffset.y = ay - ayCanvas * zoomLevel;

            clampPan();
            updateZoomTransform();
        }
        function adjustZoom(delta) { zoomTo(zoomLevel + delta, getDocumentCenterClient()); }
        function zoomToFit() {
            const centerEl = document.querySelector('.center');
            const canvas = document.getElementById('canvas');

            const availW = centerEl.clientWidth - FIT_PADDING.left - FIT_PADDING.right;
            const availH = centerEl.clientHeight - FIT_PADDING.top - FIT_PADDING.bottom;

            const cw = canvas.offsetWidth;
            const ch = canvas.offsetHeight;

            const scale = Math.max(MIN_ZOOM, Math.min(MAX_ZOOM, Math.min(availW / cw, availH / ch)));
            zoomLevel = scale;

            const visibleW = cw * zoomLevel;
            const visibleH = ch * zoomLevel;

            // Shift artboard to the left by reducing the left offset
            const leftShift = 20; // Adjust this value to shift more/less to the left
            panOffset.x = Math.round(FIT_PADDING.left + (availW - visibleW) / 2 - leftShift);
            panOffset.y = Math.round(FIT_PADDING.top + (availH - visibleH) / 2);

            updateZoomTransform();
        }
        function updateZoomTransform() {
            clampPan();
            const cont = document.getElementById('canvasContainer');
            cont.style.transform = `translate(${panOffset.x}px, ${panOffset.y}px) scale(${zoomLevel})`;
            document.getElementById('zoomLevel').textContent = Math.round(zoomLevel * 100) + '%';
        }

        /* ===== Pointer handlers ===== */
        let pointerDownOnCanvas = false;
        let movedSinceDown = false;
        let downPos = { x: 0, y: 0 };
        let lastCreateAt = 0;
        const CREATE_DEBOUNCE_MS = 250;

        function handlePointerDown(e) {
            if (currentTool === 'pan') return;

            // Always track for create-on-click logic
            pointerDownOnCanvas = true;
            movedSinceDown = false;
            downPos = clientToCanvas(e);

            const el = e.target.closest('.draggable-element');
            if (el) {
                selectElement(el);
                // Only drag if the mousedown originated on the drag handle
                if (e.target.classList.contains('drag-handle')) {
                    startDrag(e, el);
                }
                return; // don't create if you clicked an existing element
            } else {
                // Clicked on empty area - deselect current element
                if (selectedElement) {
                    selectedElement.classList.remove('selected');
                    selectedElement = null;
                    updatePropertiesPanel();
                    updateLayersList(); // Update layers to remove highlight
                }
            }
        }

        function handlePointerMove(e) {
            if (!pointerDownOnCanvas) return;
            const p = clientToCanvas(e);
            if (Math.abs(p.x - downPos.x) > 2 || Math.abs(p.y - downPos.y) > 2) movedSinceDown = true;
            if (isDragging) {
                dragMove(e);
                // Don't deselect during dragging
                return;
            }
        }
        function handlePointerUp(e) {
            if (isDragging) { endDrag(); pushHistory(); }

            // CREATE ON ANY CLICK (not limited to empty area), centered
            if (currentTool !== 'select' && currentTool !== 'pan' && !movedSinceDown) {
                const now = Date.now();
                if (now - lastCreateAt >= CREATE_DEBOUNCE_MS) {
                    lastCreateAt = now;
                    createByToolAtCenter();
                }
            }
            pointerDownOnCanvas = false;
        }

        function attachDragHandle(el) {
            // Avoid duplicates if called twice (e.g. after load)
            if (el.querySelector('.drag-handle')) return;

            const handle = document.createElement('div');
            handle.className = 'drag-handle';
            handle.title = 'Drag / Move';
            handle.textContent = '⠿'; // you can use '↕', '⤧', or an SVG if you prefer

            // Start drag ONLY from the handle (so text body remains editable)
            handle.addEventListener('mousedown', (e) => {
                e.stopPropagation();           // don't bubble into element body
                e.preventDefault();            // prevent default behavior
                if (el.dataset.locked === 'true') { selectElement(el); return; }
                startDrag(e, el);
            });

            el.appendChild(handle);
        }

        /* ===== Element creation ===== */
        function canvasCenter() { return { cx: 794 / 2, cy: 1123 / 2 }; }
        function defaultSizeForTool(tool) {
            switch (tool) {
                case 'text': return { w: 260, h: 72 };
                case 'image': return { w: 240, h: 240 };
                case 'placeholder': return { w: 260, h: 60 };
                case 'rectangle': return { w: 140, h: 100 };
                case 'circle': return { w: 140, h: 140 };
                default: return { w: 140, h: 100 };
            }
        }
        // Where you create by tool at center:
        function createByToolAtCenter() {
            const def = defaultSizeForTool(currentTool);
            const { cx, cy } = canvasCenter();
            const x = Math.round(cx - def.w / 2);
            const y = Math.round(cy - def.h / 2);

            switch (currentTool) {
                case 'text':
                    createTextElement(x, y, def.w, def.h);
                    setActiveTool('select'); // Auto-switch back to select
                    break;
                case 'image': {
                    // Image popup is now handled in setupUI(), so this case should not be reached
                    showImageUrlModal();
                    break;
                }
                case 'placeholder':
                    createDataElement(x, y, def.w, def.h);
                    setActiveTool('select'); // Auto-switch back to select
                    break;
                case 'rectangle':
                    createRectangleElement(x, y, def.w, def.h);
                    setActiveTool('select'); // Auto-switch back to select
                    break;
                case 'circle':
                    createCircleElement(x, y, def.w, def.h);
                    setActiveTool('select'); // Auto-switch back to select
                    break;
            }

            pushHistory();
        }


        function wrapCommon(el, x, y, w, h, type) {
            el.style.left = x + 'px'; el.style.top = y + 'px';
            el.style.width = w + 'px'; el.style.height = h + 'px';
            el.dataset.type = type; el.dataset.id = ++elementCounter; el.dataset.locked = 'false';
            addResizeHandles(el);
            document.getElementById('canvas').appendChild(el);
            elements.push({ id: elementCounter, type, element: el });
            selectElement(el);
            updateLayersList();
        }
        function createTextElement(x, y, w = 240, h = 64) {
            const el = document.createElement('div');
            el.className = 'draggable-element';
            const ta = document.createElement('textarea');
            ta.className = 'text-element';
            ta.placeholder = 'Enter text here...';
            el.appendChild(ta);
            wrapCommon(el, x, y, w, h, 'text');
            attachDragHandle(el); // NEW
        }
        // Data element acts like text but has metadata (field/default) and dashed container
        function createDataElement(x, y, w = 240, h = 56) {
            const el = document.createElement('div');
            el.className = 'draggable-element data-element';
            el.dataset.field = '';
            el.dataset.defaultValue = '';
            const ta = document.createElement('textarea');
            ta.className = 'text-element';
            ta.placeholder = 'Data Placeholder';
            el.appendChild(ta);
            wrapCommon(el, x, y, w, h, 'placeholder');
            attachDragHandle(el);
        }
        function createRectangleElement(x, y, w = 120, h = 90) {
            const el = document.createElement('div');
            el.className = 'draggable-element';
            el.style.backgroundColor = '#cccccc';
            wrapCommon(el, x, y, w, h, 'rectangle');
            attachDragHandle(el); // NEW
        }
        function createCircleElement(x, y, w = 120, h = 120) {
            const el = document.createElement('div');
            el.className = 'draggable-element';
            el.style.backgroundColor = '#cccccc';
            el.style.borderRadius = '50%';
            wrapCommon(el, x, y, w, h, 'circle');
            attachDragHandle(el); // NEW
        }
        function addResizeHandles(element) {
            ['nw', 'ne', 'sw', 'se'].forEach(handle => {
                const h = document.createElement('div');
                h.className = `resize-handle ${handle}`;
                h.addEventListener('mousedown', (e) => {
                    e.stopPropagation();
                    startResize(e, element, handle);
                });
                element.appendChild(h);
            });
        }

        /* ===== Selection & properties ===== */
        function selectElement(element) {
            if (selectedElement) selectedElement.classList.remove('selected');
            selectedElement = element;
            if (selectedElement) selectedElement.classList.add('selected');
            updatePropertiesPanel();
            // Update layer highlighting without recreating the list
            updateLayerHighlighting();
        }
        function updatePropertiesPanel() {
            if (!selectedElement) {
                // Show tips and hide properties when no element is selected
                document.getElementById('propertiesTip').style.display = 'block';
                document.getElementById('propertiesContent').style.display = 'none';
                document.getElementById('scrollTip').style.display = 'block';
                return;
            }

            // Hide tips and show properties when element is selected
            document.getElementById('propertiesTip').style.display = 'none';
            document.getElementById('propertiesContent').style.display = 'block';
            document.getElementById('scrollTip').style.display = 'none';
            document.getElementById('posX').value = parseInt(selectedElement.style.left) || 0;
            document.getElementById('posY').value = parseInt(selectedElement.style.top) || 0;
            document.getElementById('width').value = parseInt(selectedElement.style.width) || 0;
            document.getElementById('height').value = parseInt(selectedElement.style.height) || 0;

            const type = selectedElement.dataset.type;
            // Text controls show for both text and placeholder
            const showTextControls = (type === 'text' || type === 'placeholder');
            document.getElementById('textProperties').style.display = showTextControls ? 'block' : 'none';
            document.getElementById('placeholderProperties').style.display = (type === 'placeholder') ? 'block' : 'none';

            if (showTextControls) {
                const ta = selectedElement.querySelector('.text-element');
                document.getElementById('fontFamily').value = ta.style.fontFamily || 'Arial, sans-serif';
                const fsz = parseInt(ta.style.fontSize) || 16;
                document.getElementById('fontSize').value = fsz;
                document.getElementById('fontSizeNumber').value = fsz;
                document.getElementById('fontWeight').value = ta.style.fontWeight || 'normal';
                document.getElementById('textColor').value = cssColorToHex(ta.style.color || '#000000');
                document.getElementById('textAlign').value = ta.style.textAlign || 'left';
                setToggleVisual('textTransparent', (ta.style.color === 'transparent'));
            }
            if (type === 'placeholder') {
                const ddl = document.getElementById('dataFieldDdl');
                const customRow = document.getElementById('customFieldRow');
                const customInput = document.getElementById('dataFieldCustom');

                const field = selectedElement.dataset.field || '';

                // Check if field matches predefined options
                const predefinedOptions = ['name', 'venue', 'date', 'signature', 'eventName', 'achievement', 'recognition'];
                if (predefinedOptions.includes(field)) {
                    ddl.value = field;
                    customRow.style.display = 'none';
                } else if (field && field !== '') {
                    ddl.value = 'other';
                    customRow.style.display = 'block';
                    customInput.value = field;
                } else {
                    ddl.value = '';
                    customRow.style.display = 'none';
                }

                document.getElementById('defaultValue').value = selectedElement.dataset.defaultValue || '';
            }

            const bg = selectedElement.style.backgroundColor || '#ffffff';
            document.getElementById('backgroundColor').value = (bg === 'transparent') ? '#ffffff' : cssColorToHex(bg);
            setToggleVisual('bgTransparent', bg === 'transparent');

            const border = selectedElement.style.border || '';
            const bw = (border.match(/(\d+)px/) || [, ''])[1];
            const bc = (border.match(/rgb\([^)]+\)|#[0-9a-fA-F]{3,6}|transparent/) || [''])[0] || '#000000';
            document.getElementById('borderWidth').value = bw || '';
            document.getElementById('borderColor').value = bc === 'transparent' ? '#000000' : cssColorToHex(bc);
            setToggleVisual('borderTransparent', bc === 'transparent');

            const op = parseFloat(selectedElement.style.opacity || 1);
            document.getElementById('opacity').value = Math.round(op * 100);
            document.getElementById('opacityValue').textContent = Math.round(op * 100) + '%';
        }
        function setToggleVisual(id, on) { const btn = document.getElementById(id); if (btn) btn.classList.toggle('active', !!on); }
        function updateSelectedElement(pushHist = false) {
            if (!selectedElement) return;
            if (selectedElement.dataset.locked === 'true') return;

            const posX = document.getElementById('posX').value;
            const posY = document.getElementById('posY').value;
            const width = document.getElementById('width').value;
            const height = document.getElementById('height').value;
            if (posX !== '') selectedElement.style.left = posX + 'px';
            if (posY !== '') selectedElement.style.top = posY + 'px';
            if (width !== '') selectedElement.style.width = width + 'px';
            if (height !== '') selectedElement.style.height = height + 'px';

            const type = selectedElement.dataset.type;
            if (type === 'text' || type === 'placeholder') {
                const ta = selectedElement.querySelector('.text-element');
                ta.style.fontFamily = document.getElementById('fontFamily').value;
                const fsz = parseInt(document.getElementById('fontSize').value || '16', 10);
                ta.style.fontSize = fsz + 'px';
                document.getElementById('fontSizeNumber').value = fsz;
                ta.style.fontWeight = document.getElementById('fontWeight').value;
                ta.style.color = document.getElementById('textTransparent').classList.contains('active')
                    ? 'transparent' : document.getElementById('textColor').value;
                ta.style.textAlign = document.getElementById('textAlign').value;
            }
            if (type === 'placeholder') {
                const ddl = document.getElementById('dataFieldDdl');
                const custom = document.getElementById('dataFieldCustom');
                let field = ddl.value;
                if (field === 'other') {
                    field = custom.value.trim();
                }
                selectedElement.dataset.field = field;
                selectedElement.dataset.defaultValue = document.getElementById('defaultValue').value;
                const ta = selectedElement.querySelector('.text-element');
                if (ta && !ta.value) ta.placeholder = selectedElement.dataset.defaultValue || 'Data Placeholder';
            }

            const bgBtn = document.getElementById('bgTransparent');
            selectedElement.style.backgroundColor = bgBtn.classList.contains('active') ? 'transparent' : document.getElementById('backgroundColor').value;

            const bw = document.getElementById('borderWidth').value;
            const bcBtn = document.getElementById('borderTransparent');
            const bc = bcBtn.classList.contains('active') ? 'transparent' : document.getElementById('borderColor').value;
            if (bw) selectedElement.style.border = `${bw}px solid ${bc}`;
            else selectedElement.style.border = bc === 'transparent' ? '0 solid transparent' : '';

            selectedElement.style.opacity = (document.getElementById('opacity').value / 100);
            document.getElementById('opacityValue').textContent = document.getElementById('opacity').value + '%';

            if (pushHist) pushHistory();
        }
        function toggleBgTransparent(e) { e.preventDefault(); e.currentTarget.classList.toggle('active'); updateSelectedElement(true); }
        function toggleBorderTransparent(e) { e.preventDefault(); e.currentTarget.classList.toggle('active'); updateSelectedElement(true); }
        function toggleTextTransparent(e) { e.preventDefault(); e.currentTarget.classList.toggle('active'); updateSelectedElement(true); }

        /* ===== Dragging ===== */
        function startDrag(ev, target) {
            if (target.dataset.locked === 'true') { selectElement(target); return; }
            if (currentTool === 'pan') return;
            isDragging = true;
            selectElement(target);

            const pt = clientToCanvas(ev);
            const left = parseFloat(target.style.left) || 0;
            const top = parseFloat(target.style.top) || 0;
            dragOffset.x = pt.x - left;
            dragOffset.y = pt.y - top;

            // Prevent default to avoid any browser interference
            ev.preventDefault();
        }
        function dragMove(ev) {
            if (!isDragging || !selectedElement) return;
            if (selectedElement.dataset.locked === 'true') return;

            const cpt = clientToCanvas(ev);
            let x = cpt.x - dragOffset.x;
            let y = cpt.y - dragOffset.y;

            // Prevent default to avoid any browser interference
            ev.preventDefault();

            const curLeft = parseInt(selectedElement.style.left || '0', 10);
            const curTop = parseInt(selectedElement.style.top || '0', 10);
            if (ev.shiftKey) {
                const dx = Math.abs(x - curLeft); const dy = Math.abs(y - curTop);
                if (dx > dy) y = curTop; else x = curLeft;
            }

            if (snapEnabled) {
                const gx = Math.round(x / 10) * 10;
                const gy = Math.round(y / 10) * 10;
                if (Math.abs(gx - x) <= SNAP_PX) x = gx;
                if (Math.abs(gy - y) <= SNAP_PX) y = gy;
            }

            const rect = selectedElement.getBoundingClientRect();
            const selW = rect.width / zoomLevel, selH = rect.height / zoomLevel;
            let snapX = null, snapY = null;
            elements.forEach(el => {
                if (el.element === selectedElement || el.element.style.display === 'none') return;
                const r = el.element.getBoundingClientRect();
                const ex = parseFloat(el.element.style.left) || 0;
                const ey = parseFloat(el.element.style.top) || 0;
                const ew = r.width / zoomLevel, eh = r.height / zoomLevel;
                const edgesX = [ex, ex + ew / 2, ex + ew];
                const edgesY = [ey, ey + eh / 2, ey + eh];
                edgesX.forEach(val => {
                    if (Math.abs(val - x) <= SNAP_PX) snapX = val;
                    if (Math.abs(val - (x + selW)) <= SNAP_PX) snapX = val - selW;
                    if (Math.abs(val - (x + selW / 2)) <= SNAP_PX) snapX = val - selW / 2;
                });
                edgesY.forEach(val => {
                    if (Math.abs(val - y) <= SNAP_PX) snapY = val;
                    if (Math.abs(val - (y + selH)) <= SNAP_PX) snapY = val - selH;
                    if (Math.abs(val - (y + selH / 2)) <= SNAP_PX) snapY = val - selH / 2;
                });
            });
            if (snapX !== null) x = snapX;
            if (snapY !== null) y = snapY;
            drawGuides(snapX !== null ? x : null, snapY !== null ? y : null, selW, selH);

            selectedElement.style.left = Math.max(0, Math.round(x)) + 'px';
            selectedElement.style.top = Math.max(0, Math.round(y)) + 'px';
            updatePropertiesPanel();
        }
        function endDrag() { isDragging = false; clearGuides(); }

        /* ===== Resize ===== */
        function startResize(e, element, handle) {
            if (element.dataset.locked === 'true') return;
            e.preventDefault(); e.stopPropagation();

            const start = clientToCanvas(e);
            const cs = window.getComputedStyle(element);
            const startWidth = parseFloat(cs.width) || 0;
            const startHeight = parseFloat(cs.height) || 0;
            const startLeft = parseFloat(element.style.left || '0') || 0;
            const startTop = parseFloat(element.style.top || '0') || 0;
            const MIN = 20;
            const aspect = startWidth / Math.max(1, startHeight);

            function move(ev) {
                const pt = clientToCanvas(ev);
                const dx = pt.x - start.x;
                const dy = pt.y - start.y;

                let newWidth = startWidth;
                let newHeight = startHeight;
                let newLeft = startLeft;
                let newTop = startTop;

                switch (handle) {
                    case 'se': newWidth = Math.max(MIN, startWidth + dx); newHeight = Math.max(MIN, startHeight + dy); break;
                    case 'sw': newWidth = Math.max(MIN, startWidth - dx); newHeight = Math.max(MIN, startHeight + dy); newLeft = startLeft + dx; break;
                    case 'ne': newWidth = Math.max(MIN, startWidth + dx); newHeight = Math.max(MIN, startHeight - dy); newTop = startTop + dy; break;
                    case 'nw': newWidth = Math.max(MIN, startWidth - dx); newHeight = Math.max(MIN, startHeight - dy); newLeft = startLeft + dx; newTop = startTop + dy; break;
                }

                if (ev.shiftKey) {
                    if (newWidth / newHeight > aspect) newWidth = Math.round(newHeight * aspect);
                    else newHeight = Math.round(newWidth / aspect);
                    if (handle === 'nw' || handle === 'sw') newLeft = startLeft + (startWidth - newWidth);
                    if (handle === 'nw' || handle === 'ne') newTop = startTop + (startHeight - newHeight);
                }

                element.style.width = Math.round(newWidth) + 'px';
                element.style.height = Math.round(newHeight) + 'px';
                element.style.left = Math.round(newLeft) + 'px';
                element.style.top = Math.round(newTop) + 'px';

                updatePropertiesPanel();
            }
            function up() { document.removeEventListener('mousemove', move); document.removeEventListener('mouseup', up); pushHistory(); }
            document.addEventListener('mousemove', move);
            document.addEventListener('mouseup', up);
        }

        /* ===== Context menu ===== */
        function handleContextMenu(e) {
            e.preventDefault();
            const el = e.target.closest('.draggable-element');
            if (!el) return;
            selectElement(el);
            const m = document.getElementById('contextMenu');
            m.style.display = 'block';
            m.style.left = e.pageX + 'px';
            m.style.top = e.pageY + 'px';
        }
        function hideContextMenu() { document.getElementById('contextMenu').style.display = 'none'; }

        /* ===== Keyboard ===== */
        function handleKeyDown(e) {
            const activeTA = document.activeElement && document.activeElement.classList.contains('text-element');
            if (activeTA) {
                if (e.key === 'Escape') { document.activeElement.blur(); }
                if (e.key === 'Delete') { e.preventDefault(); deleteElement(); pushHistory(); }
                return;
            }
            if (!selectedElement) return;

            const step = e.shiftKey ? 10 : 1;
            const n = v => (parseInt(v || '0', 10) || 0);
            if (selectedElement.dataset.locked === 'true') return;

            switch (e.key) {
                case 'Delete': deleteElement(); pushHistory(); break;
                case 'ArrowLeft': selectedElement.style.left = (n(selectedElement.style.left) - step) + 'px'; updatePropertiesPanel(); pushHistory(); break;
                case 'ArrowRight': selectedElement.style.left = (n(selectedElement.style.left) + step) + 'px'; updatePropertiesPanel(); pushHistory(); break;
                case 'ArrowUp': selectedElement.style.top = (n(selectedElement.style.top) - step) + 'px'; updatePropertiesPanel(); pushHistory(); break;
                case 'ArrowDown': selectedElement.style.top = (n(selectedElement.style.top) + step) + 'px'; updatePropertiesPanel(); pushHistory(); break;
                case 'd': case 'D':
                    if ((e.ctrlKey || e.metaKey)) { e.preventDefault(); duplicateElement(); pushHistory(); }
                    break;
            }
        }

        /* ===== Layers ===== */
        function updateLayerHighlighting() {
            // Update highlighting without recreating the list
            const list = document.getElementById('layersList');
            const layerItems = list.querySelectorAll('.layer-item');
            layerItems.forEach((item, index) => {
                const reversedElements = [...elements].reverse();
                const element = reversedElements[index];
                if (element && selectedElement === element.element) {
                    item.classList.add('selected');
                } else {
                    item.classList.remove('selected');
                }
            });
        }

        function updateLayersList() {
            const list = document.getElementById('layersList');
            list.innerHTML = '';
            const arr = [...elements].reverse();
            arr.forEach(el => {
                const item = document.createElement('div');
                item.className = 'layer-item';
                item.draggable = true;
                if (selectedElement === el.element) item.classList.add('selected');
                const customName = el.element.dataset.name;
                const displayName = customName || `${el.type.charAt(0).toUpperCase() + el.type.slice(1)} ${el.id}`;
                item.innerHTML = `
                <span class="layer-visibility" title="Toggle visibility">${el.element.style.display === 'none' ? '🚫' : '👁️'}</span>
                <span class="layer-lock" title="Lock/Unlock">${el.element.dataset.locked === 'true' ? '🔒' : '🔓'}</span>
                <span class="layer-name">${displayName}</span>
              `;

                item.addEventListener('dragstart', ev => {
                    item.classList.add('dragging');
                    ev.dataTransfer.setData('text/plain', String(el.id));
                });
                item.addEventListener('dragend', () => item.classList.remove('dragging'));
                item.addEventListener('dragover', ev => ev.preventDefault());
                item.addEventListener('drop', ev => {
                    ev.preventDefault();
                    const draggedId = parseInt(ev.dataTransfer.getData('text/plain'), 10);
                    const targetId = el.id;
                    reorderLayersByIds(draggedId, targetId, arr);
                });

                const eye = item.querySelector('.layer-visibility');
                eye.addEventListener('click', (ev) => {
                    ev.stopPropagation();
                    const vis = el.element.style.display !== 'none';
                    el.element.style.display = vis ? 'none' : 'block';
                    eye.textContent = vis ? '🚫' : '👁️';
                    pushHistory();
                });

                const lock = item.querySelector('.layer-lock');
                lock.addEventListener('click', (ev) => {
                    ev.stopPropagation();
                    const locked = el.element.dataset.locked === 'true';
                    el.element.dataset.locked = (!locked).toString();
                    lock.textContent = (!locked) ? '🔒' : '🔓'; // show closed when locked
                });

                // Select on click
                item.addEventListener('click', () => selectElement(el.element));

                // Double-click to rename
                item.querySelector('.layer-name').addEventListener('dblclick', (ev) => {
                    ev.stopPropagation();
                    selectElement(el.element);
                    showLayerRenamePrompt();
                });

                // Right-click context menu for layer
                item.addEventListener('contextmenu', (ev) => {
                    ev.preventDefault();
                    selectElement(el.element);
                    showContextMenu(ev, 'layer');
                });

                list.appendChild(item);
            });
            normalizeZ();
        }
        function reorderLayersByIds(draggedId, targetId) {
            const from = elements.findIndex(e => e.id === draggedId);
            const to = elements.findIndex(e => e.id === targetId);
            if (from === -1 || to === -1 || from === to) return;

            const item = elements.splice(from, 1)[0];
            elements.splice(to + 1, 0, item); // move above target

            const canvas = document.getElementById('canvas');
            const grid = canvas.firstElementChild; // keep grid
            canvas.innerHTML = ''; canvas.appendChild(grid);
            elements.forEach(e => canvas.appendChild(e.element));
            updateLayersList();
            pushHistory();
        }
        function normalizeZ() { let z = 1; elements.forEach(e => { e.element.style.zIndex = String(z++); }); }
        function bringToFront() {
            if (!selectedElement) return;
            const idx = elements.findIndex(e => e.element === selectedElement);
            if (idx === -1 || idx === elements.length - 1) return;
            const item = elements.splice(idx, 1)[0];
            elements.push(item);
            document.getElementById('canvas').appendChild(selectedElement);
            updateLayersList(); pushHistory();
        }
        function sendToBack() {
            if (!selectedElement) return;
            const idx = elements.findIndex(e => e.element === selectedElement);
            if (idx <= 0) return;
            const item = elements.splice(idx, 1)[0];
            elements.unshift(item);
            const canvas = document.getElementById('canvas');
            canvas.insertBefore(selectedElement, canvas.firstElementChild.nextElementSibling); // after grid
            updateLayersList(); pushHistory();
        }
        function duplicateElement() {
            if (!selectedElement) return;
            if (selectedElement.dataset.locked === 'true') return;
            const clone = selectedElement.cloneNode(true);
            clone.dataset.id = ++elementCounter;
            clone.style.left = (parseInt(selectedElement.style.left) + 20) + 'px';
            clone.style.top = (parseInt(selectedElement.style.top) + 20) + 'px';
            clone.dataset.locked = 'false';
            clone.querySelectorAll('.resize-handle').forEach(h => h.remove());
            addResizeHandles(clone);
            document.getElementById('canvas').appendChild(clone);
            elements.push({ id: elementCounter, type: clone.dataset.type, element: clone });
            selectElement(clone); updateLayersList();
        }
        function deleteElement() {
            if (!selectedElement) return;
            const id = parseInt(selectedElement.dataset.id, 10);
            elements = elements.filter(el => el.id !== id);
            selectedElement.remove();
            selectedElement = null;
            updateLayersList();
        }
        function renameElement() {
            if (!selectedElement) return;
            const newName = prompt('Enter new name for this element:', selectedElement.dataset.name || '');
            if (newName !== null) {
                selectedElement.dataset.name = newName;
                updateLayersList();
            }
        }
        function handleLayersPanelClick(e) {
            // If clicked on empty area (not on a layer item), deselect
            if (e.target.id === 'layersList') {
                if (selectedElement) {
                    selectedElement.classList.remove('selected');
                    selectedElement = null;
                    updatePropertiesPanel();
                    updateLayersList(); // Update to remove selection highlight
                }
            }
        }

        /* ===== Guides ===== */
        function drawGuides(x, y, w, h) {
            const c = document.getElementById('canvas');
            if (!guideV) { guideV = document.createElement('div'); guideV.style.position = 'absolute'; guideV.style.top = '0'; guideV.style.bottom = '0'; guideV.style.width = '1px'; guideV.style.background = 'rgba(0,123,255,.7)'; guideV.style.pointerEvents = 'none'; c.appendChild(guideV); }
            if (!guideH) { guideH = document.createElement('div'); guideH.style.position = 'absolute'; guideH.style.left = '0'; guideH.style.right = '0'; guideH.style.height = '1px'; guideH.style.background = 'rgba(0,123,255,.7)'; guideH.style.pointerEvents = 'none'; c.appendChild(guideH); }
            guideV.style.display = (x === null) ? 'none' : 'block';
            guideH.style.display = (y === null) ? 'none' : 'block';
            if (x !== null) guideV.style.left = (x + w / 2) + 'px';
            if (y !== null) guideH.style.top = (y + h / 2) + 'px';
        }
        function clearGuides() { if (guideV) guideV.style.display = 'none'; if (guideH) guideH.style.display = 'none'; }

        /* ===== Image Upload ===== */
        function handleImageUpload(e) {
            const file = e.target.files[0];
            if (!file) return;
            const reader = new FileReader();
            reader.onload = function (event) {
                const el = document.createElement('div');
                el.className = 'draggable-element image-element';
                const p = window.pendingImagePosition || centerDefaultFor('image');
                wrapCommon(el, p.x, p.y, p.w, p.h, 'image');
                el.style.backgroundImage = `url(${event.target.result})`;
                pushHistory();
            };
            reader.readAsDataURL(file);
        }
        function centerDefaultFor(tool) {
            const def = defaultSizeForTool(tool);
            const { cx, cy } = canvasCenter();
            return { x: Math.round(cx - def.w / 2), y: Math.round(cy - def.h / 2), w: def.w, h: def.h };
        }

        /* ===== Save / Load / Export ===== */
        function serializeState() {
            return {
                elements: elements.map(el => ({
                    id: el.id,
                    type: el.type,
                    style: {
                        left: el.element.style.left,
                        top: el.element.style.top,
                        width: el.element.style.width,
                        height: el.element.style.height,
                        backgroundColor: el.element.style.backgroundColor,
                        border: el.element.style.border,
                        borderRadius: el.element.style.borderRadius,
                        zIndex: el.element.style.zIndex,
                        opacity: el.element.style.opacity,
                        display: el.element.style.display || 'block'
                    },
                    text: el.type === 'text' ? {
                        value: (el.element.querySelector('.text-element')?.value || ''),
                        fontFamily: (el.element.querySelector('.text-element')?.style.fontFamily || ''),
                        fontSize: (el.element.querySelector('.text-element')?.style.fontSize || ''),
                        fontWeight: (el.element.querySelector('.text-element')?.style.fontWeight || ''),
                        color: (el.element.querySelector('.text-element')?.style.color || ''),
                        textAlign: (el.element.querySelector('.text-element')?.style.textAlign || '')
                    } : null,
                    dataField: el.element.dataset.field,
                    defaultValue: el.element.dataset.defaultValue,
                    imageUrl: el.element.dataset.imageUrl || null,      // <-- store URL
                    // Keep backgroundImage for backward compat (optional)
                    backgroundImage: undefined,
                    locked: el.element.dataset.locked === 'true'
                })),
                meta: { zoomLevel, panOffset }
            };
        }

        function applyState(state) {
            if (!state) return;
            historyHold = true;

            elements.forEach(el => el.element.remove());
            elements = []; selectedElement = null;

            const canvas = document.getElementById('canvas');
            canvas.innerHTML = '<div class="grid"></div>';

            (state.elements || []).forEach(d => {
                let el = document.createElement('div');
                el.className = 'draggable-element';
                el.dataset.type = d.type;
                el.dataset.id = d.id;
                el.dataset.locked = d.locked ? 'true' : 'false';

                Object.assign(el.style, d.style || {});
                if (d.type === 'circle') el.style.borderRadius = '50%';
                if (d.type === 'image') {
                    el.classList.add('image-element');
                    if (d.imageUrl) {
                        el.dataset.imageUrl = d.imageUrl;
                        const img = document.createElement('img');
                        img.className = 'image-content';
                        img.src = d.imageUrl;
                        el.appendChild(img);
                    } else if (d.backgroundImage) {
                        const m = d.backgroundImage.match(/url\((.*)\)/i);
                        const src = m ? m[1].replace(/^\"|\"$/g, '') : '';
                        if (src) {
                            const img = document.createElement('img');
                            img.className = 'image-content';
                            img.src = src;
                            el.appendChild(img);
                            el.dataset.imageUrl = src;
                        }
                    }
                }
                if (d.type === 'placeholder') {
                    el.classList.add('data-element');
                    if (d.dataField) el.dataset.field = d.dataField;
                    if (d.defaultValue) el.dataset.defaultValue = d.defaultValue;
                    const ta = document.createElement('textarea');
                    ta.className = 'text-element';
                    if (d.text) {
                        ta.value = d.text.value || '';
                        ta.style.fontFamily = d.text.fontFamily || '';
                        ta.style.fontSize = d.text.fontSize || '';
                        ta.style.fontWeight = d.text.fontWeight || '';
                        ta.style.color = d.text.color || '';
                        ta.style.textAlign = d.text.textAlign || '';
                    }
                    el.appendChild(ta);
                } else if (d.type === 'text') {
                    const ta = document.createElement('textarea');
                    ta.className = 'text-element';
                    ta.value = d.text?.value || '';
                    if (d.text) {
                        ta.style.fontFamily = d.text.fontFamily || '';
                        ta.style.fontSize = d.text.fontSize || '';
                        ta.style.fontWeight = d.text.fontWeight || '';
                        ta.style.color = d.text.color || '';
                        ta.style.textAlign = d.text.textAlign || '';
                    }
                    el.appendChild(ta);
                }
                if (d.style?.backgroundImage) el.classList.add('image-element');
                addResizeHandles(el);
                attachDragHandle(el);
                canvas.appendChild(el);
                elements.push({ id: d.id, type: d.type, element: el });
            });

            elementCounter = elements.reduce((m, el) => Math.max(m, el.id), 0);
            // Always reset view after loading
            zoomToFit();
            updateZoomTransform();
            updateLayersList();
            historyHold = false;
        }
        function saveTemplate() {
            const data = serializeState();
            const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' });
            const url = URL.createObjectURL(blob); const a = document.createElement('a');
            a.href = url; a.download = 'template.json'; a.click(); URL.revokeObjectURL(url);
            showSuccessMessage('Template saved locally!');
        }

        // New function to save template data to server-side hidden fields
        function saveTemplateToServer() {
            const data = serializeState();
            document.getElementById('<%= hdnTemplateData.ClientID %>').value = JSON.stringify(data);
          let templateName = '';
          let templateDescription = '';
          let templateType = '';
          if (window.parent && typeof window.parent.getTemplateMeta === 'function') {
              const meta = window.parent.getTemplateMeta();
              templateName = meta.templateName || '';
              templateType = meta.templateType || '';
              templateDescription = meta.templateDescription || '';
          }
          document.getElementById('<%= hdnTemplateName.ClientID %>').value = templateName;
          document.getElementById('<%= hdnTemplateDescription.ClientID %>').value = templateDescription;
          document.getElementById('<%= hdnTemplateType.ClientID %>').value = templateType;
          const backgroundImage = document.querySelector('.image-element[data-image-url]');
          if (backgroundImage) {
              document.getElementById('<%= hdnTemplateImageUrl.ClientID %>').value = backgroundImage.dataset.imageUrl;
            }
            // AJAX submit
            const form = document.getElementById('form1');
            const formData = new FormData(form);
            fetch(form.action, {
                method: 'POST',
                body: formData,
                credentials: 'same-origin'
            }).then(response => {
                if (response.ok) {
                    showSuccessMessage('Template saved to server successfully!');
                } else {
                    showSuccessMessage('Server error: could not save.');
                }
            }).catch(() => {
                showSuccessMessage('Network error: could not save.');
            });
        }

        // Function to load template data from server
        function loadTemplateFromServer(templateData) {
            if (templateData) {
                try {
                    const data = JSON.parse(templateData);
                    applyState(data);
                    history = [serializeState()];
                    historyIndex = 0;
                    setUndoRedoState();
                    // Always reset view after loading
                    zoomToFit();
                } catch (err) {
                    console.error('Error loading template data: ' + err.message);
                }
            }
        }

        // Modal functions
        function showImageUrlModal() {
            const modal = document.createElement('div');
            modal.className = 'modal-overlay';
            modal.innerHTML = `
              <div class="modal">
                  <h3>Add Image</h3>
                  <p>Paste a public image URL (Google Drive links supported):</p>
                  <input type="url" id="imageUrlInput" placeholder="https://example.com/image.jpg" autofocus>
                  <div class="modal-buttons">
                      <button class="modal-btn modal-btn-secondary" onclick="closeModal()">Cancel</button>
                      <button class="modal-btn modal-btn-primary" onclick="addImageFromModal()">Add Image</button>
                  </div>
              </div>
          `;
            document.body.appendChild(modal);

            // Focus on input and handle Enter key
            const input = modal.querySelector('#imageUrlInput');
            input.addEventListener('keypress', (e) => {
                if (e.key === 'Enter') addImageFromModal();
            });
        }

        function addImageFromModal() {
            const input = document.querySelector('#imageUrlInput');
            const url = input.value.trim();
            if (url) {
                createImageFromUrl(url);
                closeModal();
                // Tool is already switched to select in createImageFromUrl, so no need to do it here
            }
        }
        function closeModal() {
            const modal = document.querySelector('.modal-overlay');
            if (modal) modal.remove();
        }

        function showSuccessMessage(message) {
            const success = document.createElement('div');
            success.className = 'success-message';
            success.textContent = message;
            document.body.appendChild(success);

            setTimeout(() => {
                success.remove();
            }, 3000);
        }

        function showCloseWarning() {
            const modal = document.createElement('div');
            modal.className = 'modal-overlay';
            modal.innerHTML = `
              <div class="modal">
                  <h3>Close Template Editor</h3>
                  <p>You will lose all unsaved progress. Would you like to save your work locally before closing?</p>
                  <div class="modal-buttons">
                      <button class="modal-btn modal-btn-secondary" onclick="closeModal()">Cancel</button>
                      <button class="modal-btn modal-btn-danger" onclick="closeWithoutSaving()">Close Without Saving</button>
                      <button class="modal-btn modal-btn-primary" onclick="saveAndClose()">Save & Close</button>
                  </div>
              </div>
          `;
            document.body.appendChild(modal);
        }

        function closeWithoutSaving() {
            closeModal();
            window.close();
        }

        function saveAndClose() {
            saveTemplate();
            closeModal();
            showSuccessMessage('Template saved locally!');
            setTimeout(() => {
                window.close();
            }, 1000);
        }
        function loadTemplate() { document.getElementById('templateUpload').click(); }
        function handleTemplateUpload(e) {
            const file = e.target.files[0]; if (!file) return;
            const reader = new FileReader();
            reader.onload = ev => {
                try {
                    const data = JSON.parse(ev.target.result);
                    applyState(data);
                    history = [serializeState()];
                    historyIndex = 0;
                    setUndoRedoState();
                } catch (err) { alert('Error loading template: ' + err.message); }
            };
            reader.readAsText(file);
        }

        /* ===== Undo / Redo ===== */
        function deepClone(obj) { return JSON.parse(JSON.stringify(obj)); }
        function pushHistory(initial = false) {
            if (historyHold) return;
            const snap = serializeState();
            // if last snapshot equals current, skip
            if (!initial && historyIndex >= 0) {
                const last = history[historyIndex];
                if (JSON.stringify(last) === JSON.stringify(snap)) { setUndoRedoState(); return; }
            }
            if (historyIndex < history.length - 1) history = history.slice(0, historyIndex + 1);
            history.push(deepClone(snap));
            if (history.length > HISTORY_LIMIT) { history.shift(); }
            historyIndex = history.length - 1;
            setUndoRedoState();
        }
        function undo() {
            if (historyIndex <= 0) return;
            historyIndex--;
            historyHold = true;
            applyState(deepClone(history[historyIndex]));
            historyHold = false;
            setUndoRedoState();
        }
        function redo() {
            if (historyIndex >= history.length - 1) return;
            historyIndex++;
            historyHold = true;
            applyState(deepClone(history[historyIndex]));
            historyHold = false;
            setUndoRedoState();
        }
        function setUndoRedoState() {
            const u = document.getElementById('btnUndo');
            const r = document.getElementById('btnRedo');
            u.disabled = !(historyIndex > 0);
            r.disabled = !(historyIndex < history.length - 1);
        }

        /* ===== Utils ===== */
        function cssColorToHex(color) {
            if (!color) return '#000000';
            if (color === 'transparent') return '#000000';
            if (color.startsWith('#')) return color;
            const m = color.match(/\d+/g); if (!m) return '#000000';
            const r = (+m[0]).toString(16).padStart(2, '0');
            const g = (+m[1]).toString(16).padStart(2, '0');
            const b = (+m[2]).toString(16).padStart(2, '0');
            return `#${r}${g}${b}`;
        }

        // Example: data population hook
        function populateTemplate(firebaseData) {
            elements.forEach(el => {
                if (el.type === 'placeholder' && el.element.dataset.field) {
                    const path = el.element.dataset.field.split('.');
                    let value = firebaseData;
                    for (let i = 0; i < path.length; i++) {
                        if (value && Object.prototype.hasOwnProperty.call(value, path[i])) value = value[path[i]];
                        else { value = el.element.dataset.defaultValue || 'N/A'; break; }
                    }
                    const ta = el.element.querySelector('.text-element');
                    if (ta) ta.value = value;
                }
            });
        }

        function showLayerRenamePrompt() {
            if (!selectedElement) return;
            const newName = prompt('Enter new name for this layer:', selectedElement.dataset.name || '');
            if (newName !== null) {
                selectedElement.dataset.name = newName;
                updateLayersList();
            }
        }

        function showContextMenu(e, type) {
            const m = document.getElementById('contextMenu');
            m.style.display = 'block';
            m.style.left = e.pageX + 'px';
            m.style.top = e.pageY + 'px';
            // Show/hide options based on type
            document.getElementById('ctx-rename').style.display = (type === 'layer') ? '' : 'none';
            document.getElementById('ctx-moveUp').style.display = (type === 'layer') ? '' : 'none';
            document.getElementById('ctx-moveDown').style.display = (type === 'layer') ? '' : 'none';
        }

        // Override context menu for canvas objects
        function handleContextMenu(e) {
            e.preventDefault();
            const el = e.target.closest('.draggable-element');
            if (!el) return;
            selectElement(el);
            showContextMenu(e, 'object');
        }

        function moveLayerUp() {
            if (!selectedElement) return;
            const idx = elements.findIndex(e => e.element === selectedElement);
            if (idx === -1 || idx === elements.length - 1) return;
            const item = elements.splice(idx, 1)[0];
            elements.splice(idx + 1, 0, item);
            document.getElementById('canvas').appendChild(selectedElement);
            updateLayersList(); pushHistory();
        }
        function moveLayerDown() {
            if (!selectedElement) return;
            const idx = elements.findIndex(e => e.element === selectedElement);
            if (idx <= 0) return;
            const item = elements.splice(idx, 1)[0];
            elements.splice(idx - 1, 0, item);
            const canvas = document.getElementById('canvas');
            canvas.insertBefore(selectedElement, canvas.firstElementChild.nextElementSibling); // after grid
            updateLayersList(); pushHistory();
        }
    </script>
  </form>
</body>
</html>
