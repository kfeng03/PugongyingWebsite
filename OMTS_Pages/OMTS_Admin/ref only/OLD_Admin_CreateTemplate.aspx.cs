using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Newtonsoft.Json;

namespace fyp
{
    public partial class CertificateAnalyzer : System.Web.UI.Page
    {
        private readonly string _geminiApiKey = "AIzaSyCRxx_Bnp_KrVbl3Y54TKitULH4qBEnMvE";
        protected CloudinaryHelper cloudinaryHelper;
        protected FirebaseHelper firebaseHelper;

        // A4 standard dimensions in pixels (at 96 DPI)
        private const int A4_WIDTH = 794;  // 8.27 inches * 96 DPI
        private const int A4_HEIGHT = 1123; // 11.69 inches * 96 DPI

        protected void Page_Load(object sender, EventArgs e)
        {
            // Initialize helpers
            cloudinaryHelper = new CloudinaryHelper();
            firebaseHelper = new FirebaseHelper();

            if (!IsPostBack)
            {
                // Hide all popups by default
                HideAllPopups();

                // Setup orientation dropdown
                if (rblOrientation.Items.Count == 0)
                {
                    rblOrientation.Items.Add(new ListItem("Portrait (Vertical)", "portrait"));
                    rblOrientation.Items.Add(new ListItem("Landscape (Horizontal)", "landscape"));
                    rblOrientation.SelectedValue = "portrait";
                }

                // Check if in edit mode
                string mode = Request.QueryString["mode"];
                if (mode == "edit" && Session["EditingTemplateId"] != null)
                {
                    // Load existing template data
                    LoadExistingTemplate();
                }
            }
            else
            {
                // This is important - on postback, if we have field data in the hidden field, update our session
                if (!string.IsNullOrEmpty(hdnFieldsData.Value))
                {
                    UpdateFieldCoordinatesFromClient();
                }
            }
        }

        private async void LoadExistingTemplate()
        {
            try
            {
                // Retrieve template details from session
                string templateId = Session["EditingTemplateId"]?.ToString();
                LogDebug($"Loading template ID: {templateId}");

                // Fetch the full template details
                var template = await firebaseHelper.GetTemplateById(templateId);

                if (template == null)
                {
                    ShowMessage("Template not found.", MessageType.Error);
                    return;
                }

                // Determine orientation based on dimensions
                bool isPortrait = template.ImageWidth < template.ImageHeight;
                LogDebug($"Template dimensions: {template.ImageWidth}x{template.ImageHeight}, isPortrait: {isPortrait}");

                // Set orientation dropdown
                rblOrientation.SelectedValue = isPortrait ? "portrait" : "landscape";

                // Set template name and type
                txtTemplateName.Text = template.TemplateName ?? Path.GetFileNameWithoutExtension(template.TemplateName);
                if (!string.IsNullOrEmpty(template.TemplateType))
                {
                    rblTemplateType.SelectedValue = template.TemplateType;
                }

                // Download the template image to a local temp file for editing
                string uploadsFolder = Server.MapPath("~/Uploads/");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                string localFileName = $"Edit_{Guid.NewGuid():N}.png";
                string localFilePath = Path.Combine(uploadsFolder, localFileName);

                using (var client = new System.Net.WebClient())
                {
                    await client.DownloadFileTaskAsync(new Uri(template.TemplateUrl), localFilePath);
                }

                // Store necessary session variables
                Session["OriginalTemplateUrl"] = template.TemplateUrl;
                Session["TemplateURL"] = $"~/Uploads/{localFileName}";
                Session["StandardizedFilePath"] = localFilePath;
                Session["FieldCoordinates"] = template.Fields;
                Session["ImageWidth"] = template.ImageWidth;
                Session["ImageHeight"] = template.ImageHeight;
                Session["IsPortrait"] = isPortrait;
                Session["TemplateName"] = template.TemplateName ?? "Untitled Template";
                Session["TemplateType"] = template.TemplateType;
                Session["IsEditing"] = true;
                Session["EditingTemplateId"] = templateId;

                // Debug logging
                LogDebug($"Loaded Template Fields: {template.Fields.Count}");
                foreach (var field in template.Fields)
                {
                    LogDebug($"Field: {field.Key}, Type: {field.Value.Type}, " +
                             $"Bounds: X={field.Value.Bounds.X}, Y={field.Value.Bounds.Y}, " +
                             $"Width={field.Value.Bounds.Width}, Height={field.Value.Bounds.Height}");
                }

                // Update UI for editing
                btnUploadAnalyze.Text = "Update Template";
                btnUploadAnalyze.Visible = false;
                btnViewResult.Visible = true;

                // Automatically show analysis result
                btnViewResult_Click(null, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                LogDebug($"Error loading existing template: {ex.Message}\n{ex.StackTrace}");
                ShowMessage($"Error loading template: {ex.Message}", MessageType.Error);
            }
        }

        private void HideAllPopups()
        {
            // Hide all popups
            overlay.Style["display"] = "none";
            analysisPopup.Style["display"] = "none";
            backgroundChangePopup.Style["display"] = "none";
        }

        private void ShowPopup(HtmlGenericControl popup)
        {
            // Hide all popups first
            HideAllPopups();

            // Show the overlay and the specified popup
            overlay.Style["display"] = "block";
            popup.Style["display"] = "block";
        }

        protected async void btnUploadAnalyze_Click(object sender, EventArgs e)
        {
            if (!fuTemplate.HasFile)
            {
                ShowMessage("Please select a certificate template file to analyze.", MessageType.Error);
                return;
            }

            try
            {
                ShowMessage("Analyzing certificate template for fields...", MessageType.Info);

                // Ensure the Uploads folder exists
                string uploadsFolder = Server.MapPath("~/Uploads/");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Get selected orientation and template info
                bool isPortrait = rblOrientation.SelectedValue == "portrait";
                string templateName = txtTemplateName.Text.Trim();
                string templateType = rblTemplateType.SelectedValue;

                if (string.IsNullOrEmpty(templateName))
                {
                    templateName = "Template " + DateTime.Now.ToString("yyyyMMdd_HHmmss");
                }

                // Standardized dimensions based on orientation
                int standardWidth = isPortrait ? A4_WIDTH : A4_HEIGHT;
                int standardHeight = isPortrait ? A4_HEIGHT : A4_WIDTH;

                // Generate a unique filename for the original upload
                string originalFileName = Guid.NewGuid().ToString("N") + Path.GetExtension(fuTemplate.FileName);
                string originalFilePath = Path.Combine(uploadsFolder, originalFileName);

                // Save the uploaded file
                fuTemplate.SaveAs(originalFilePath);

                // Save the original file path for potential later use
                Session["OriginalFilePath"] = originalFilePath;

                // Generate a standardized A4 version
                string standardizedFileName = "A4_" + originalFileName;
                string standardizedFilePath = Path.Combine(uploadsFolder, standardizedFileName);

                // Resize image to A4 dimensions
                ResizeImageToA4(originalFilePath, standardizedFilePath, isPortrait);

                // Store the URL for the standardized version
                string templateUrl = $"~/Uploads/{standardizedFileName}";
                Session["StandardizedFilePath"] = standardizedFilePath;

                // Now we know the dimensions are exactly A4 size
                int imageWidth = standardWidth;
                int imageHeight = standardHeight;

                // Convert image to base64 for Gemini API
                string base64Image = ConvertImageToBase64(standardizedFilePath);

                // Analyze the template with Gemini API
                var fieldCoordinates = await AnalyzeTemplateWithGemini(base64Image, imageWidth, imageHeight);

                // Store data in session for later use
                Session["FieldCoordinates"] = fieldCoordinates;
                Session["TemplateURL"] = templateUrl;
                Session["ImageWidth"] = imageWidth;
                Session["ImageHeight"] = imageHeight;
                Session["IsPortrait"] = isPortrait;
                Session["TemplateType"] = templateType;
                Session["TemplateName"] = templateName;

                // Build analysis result for display
                StringBuilder resultBuilder = new StringBuilder();
                resultBuilder.AppendLine($"Successfully analyzed certificate template: {fuTemplate.FileName}");
                resultBuilder.AppendLine();
                resultBuilder.AppendLine($"Template standardized to A4 {(isPortrait ? "Portrait" : "Landscape")}: {imageWidth}x{imageHeight} pixels");
                resultBuilder.AppendLine();
                resultBuilder.AppendLine($"Identified {fieldCoordinates.Count} fields:");
                resultBuilder.AppendLine();

                foreach (var field in fieldCoordinates)
                {
                    resultBuilder.AppendLine($"• {field.Key}");
                    resultBuilder.AppendLine($"  Type: {field.Value.Type}");
                    resultBuilder.AppendLine($"  Position: X={field.Value.Bounds.X}, Y={field.Value.Bounds.Y}");
                    resultBuilder.AppendLine($"  Size: {field.Value.Bounds.Width}×{field.Value.Bounds.Height}");
                    resultBuilder.AppendLine($"  Required: {(field.Value.IsRequired ? "Yes" : "No")}");
                    resultBuilder.AppendLine();
                }

                // Store analysis result in session
                Session["AnalysisResult"] = resultBuilder.ToString();

                // Show View Result button and hide Upload & Analyze button
                btnViewResult.Visible = true;
                btnUploadAnalyze.Visible = false;

                // Show success message
                if (fieldCoordinates.Count > 0)
                {
                    ShowMessage($"Certificate template analyzed successfully. Found {fieldCoordinates.Count} fields. Click 'View Analysis Result' to review and edit.", MessageType.Success);
                }
                else
                {
                    ShowMessage("No fields found. You can manually add fields in the next step.", MessageType.Warning);
                }
            }
            catch (Exception ex)
            {
                LogDebug($"Certificate analysis error: {ex.Message}\n{ex.StackTrace}");
                ShowMessage($"Error analyzing certificate template: {ex.Message}", MessageType.Error);
            }
        }

        /// <summary>
        /// Resizes an image to A4 dimensions while preserving aspect ratio
        /// </summary>
        private void ResizeImageToA4(string sourcePath, string destinationPath, bool portrait)
        {
            using (var sourceImage = System.Drawing.Image.FromFile(sourcePath))
            {
                // Target dimensions
                int targetWidth = portrait ? A4_WIDTH : A4_HEIGHT;
                int targetHeight = portrait ? A4_HEIGHT : A4_WIDTH;

                // Create a new bitmap with A4 dimensions
                using (var newImage = new Bitmap(targetWidth, targetHeight))
                {
                    // Fill with white background
                    using (var graphics = Graphics.FromImage(newImage))
                    {
                        graphics.Clear(Color.White);

                        // Set high quality
                        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        graphics.SmoothingMode = SmoothingMode.HighQuality;
                        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        graphics.CompositingQuality = CompositingQuality.HighQuality;

                        // Calculate dimensions to maintain aspect ratio
                        float ratio = Math.Min((float)targetWidth / sourceImage.Width,
                                               (float)targetHeight / sourceImage.Height);

                        int newWidth = (int)(sourceImage.Width * ratio);
                        int newHeight = (int)(sourceImage.Height * ratio);

                        // Center the image
                        int posX = (targetWidth - newWidth) / 2;
                        int posY = (targetHeight - newHeight) / 2;

                        // Draw the resized image
                        graphics.DrawImage(sourceImage, posX, posY, newWidth, newHeight);
                    }

                    // Save the new image
                    newImage.Save(destinationPath, sourceImage.RawFormat);
                }
            }
        }

        /// <summary>
        /// Analyzes the certificate template image using Gemini API
        /// </summary>
        private async Task<Dictionary<string, FieldData>> AnalyzeTemplateWithGemini(
    string base64Image, int imageWidth, int imageHeight)
        {
            var results = new Dictionary<string, FieldData>();
            var apiKey = _geminiApiKey;

            using (var client = new HttpClient())
            {
                var model = "gemini-2.5-pro"; // or "gemini-2.5-flash"
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent";

                // 1) JSON Mode schema: make the model return exactly what we expect.
                var request = new
                {
                    contents = new object[] {
                new {
                    parts = new object[] {
                        new {
                            inline_data = new {
                                mime_type = "image/jpeg", // or your actual mime type
                                data = base64Image
                            }
                        },
                        new {
                            text = $@"
You are a precise certificate-field detector. 
Return JSON ONLY (no prose). Use the schema provided.

Detect the following fields if present:
- 'Participant Name' (mandatory)
- 'Date'
- 'Certificate Title'
- 'Achievement'
- 'Signature'

Rules:
- Return bounding boxes in normalized coordinates [0..1000], as [x, y, width, height], origin top-left.
- Boxes must tightly cover the target text/line/box.
- If a field is missing, omit it (don't guess).
"
                        }
                    }
                }
            },
                    // Enforce structured JSON output
                    generationConfig = new
                    {
                        response_mime_type = "application/json",
                        response_schema = new
                        {
                            type = "object",
                            properties = new
                            {
                                fields = new
                                {
                                    type = "array",
                                    items = new
                                    {
                                        type = "object",
                                        properties = new
                                        {
                                            name = new { type = "string" },
                                            type = new { type = "string", @enum = new[] { "Text", "Date", "Number", "Signature" } },
                                            box = new
                                            {
                                                type = "array",
                                                items = new { type = "number" },
                                                minItems = 4,
                                                maxItems = 4
                                            },
                                            required = new { type = "boolean" }
                                        },
                                        required = new[] { "name", "type", "box", "required" }
                                    }
                                }
                            },
                            required = new[] { "fields" }
                        }
                    }
                };

                var json = JsonConvert.SerializeObject(request);
                var http = new HttpRequestMessage(HttpMethod.Post, url);
                http.Headers.Add("x-goog-api-key", apiKey);
                http.Content = new StringContent(json, Encoding.UTF8, "application/json");

                var resp = await client.SendAsync(http);
                var body = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                    throw new Exception($"Gemini error {(int)resp.StatusCode}: {body}");

                dynamic doc = JsonConvert.DeserializeObject(body);
                // With JSON Mode, the first candidate's text IS valid JSON. Parse it directly:
                string jsonText = (string)doc.candidates[0].content.parts[0].text;
                dynamic parsed = JsonConvert.DeserializeObject(jsonText);

                foreach (var f in parsed.fields)
                {
                    string name = (string)f.name;
                    string type = (string)f.type;
                    bool required = (bool)f.required;

                    // 2) Convert normalized box [x, y, w, h] from 0..1000 to pixels
                    //    Official guidance: divide by 1000, multiply by original W/H
                    //    (we clamp to bounds to be safe).
                    double nx = (double)f.box[0] / 1000.0;
                    double ny = (double)f.box[1] / 1000.0;
                    double nw = (double)f.box[2] / 1000.0;
                    double nh = (double)f.box[3] / 1000.0;

                    int x = Math.Max(0, Math.Min((int)Math.Round(nx * imageWidth), imageWidth - 1));
                    int y = Math.Max(0, Math.Min((int)Math.Round(ny * imageHeight), imageHeight - 1));
                    int w = Math.Max(20, Math.Min((int)Math.Round(nw * imageWidth), imageWidth - x));
                    int h = Math.Max(10, Math.Min((int)Math.Round(nh * imageHeight), imageHeight - y));

                    var rect = new Rectangle(x, y, w, h);

                    // Optional: de-dup with your overlap check
                    bool overlaps = results.Values.Any(ex =>
                        RectanglesOverlapSignificantly(rect, ex.Bounds, 0.7));

                    if (!overlaps)
                    {
                        results[name] = new FieldData
                        {
                            Type = type,
                            Label = name,
                            Bounds = rect,
                            IsRequired = required
                        };
                    }
                }
            }

            if (results.Count == 0)
                AddDefaultFields(results, imageWidth, imageHeight);

            return results;
        }

        /// <summary>
        /// Adds default fields based on typical certificate layouts
        /// </summary>
        private void AddDefaultFields(Dictionary<string, FieldData> fieldCoordinates, int imageWidth, int imageHeight)
        {
            // Title field (usually at the top)
            int titleY = (int)(imageHeight * 0.15);
            fieldCoordinates["Certificate Title"] = new FieldData
            {
                Type = "Text",
                Label = "Certificate Title",
                Bounds = new Rectangle(
                    (int)(imageWidth * 0.3),
                    titleY,
                    (int)(imageWidth * 0.4),
                    (int)(imageHeight * 0.08)),
                IsRequired = true
            };

            // Name field (center)
            int nameY = (int)(imageHeight * 0.4);
            fieldCoordinates["Participant Name"] = new FieldData
            {
                Type = "Text",
                Label = "Participant Name",
                Bounds = new Rectangle(
                    (int)(imageWidth * 0.25),
                    nameY,
                    (int)(imageWidth * 0.5),
                    (int)(imageHeight * 0.08)),
                IsRequired = true
            };

            // Achievement field
            int achievementY = (int)(imageHeight * 0.55);
            fieldCoordinates["Achievement"] = new FieldData
            {
                Type = "Text",
                Label = "Achievement",
                Bounds = new Rectangle(
                    (int)(imageWidth * 0.2),
                    achievementY,
                    (int)(imageWidth * 0.6),
                    (int)(imageHeight * 0.1)),
                IsRequired = false
            };

            // Date field
            int dateY = (int)(imageHeight * 0.75);
            fieldCoordinates["Date"] = new FieldData
            {
                Type = "Date",
                Label = "Date",
                Bounds = new Rectangle(
                    (int)(imageWidth * 0.35),
                    dateY,
                    (int)(imageWidth * 0.3),
                    (int)(imageHeight * 0.05)),
                IsRequired = false
            };

            // Signature field
            int signatureY = (int)(imageHeight * 0.82);
            fieldCoordinates["Signature"] = new FieldData
            {
                Type = "Signature",
                Label = "Signature",
                Bounds = new Rectangle(
                    (int)(imageWidth * 0.6),
                    signatureY,
                    (int)(imageWidth * 0.3),
                    (int)(imageHeight * 0.08)),
                IsRequired = false
            };
        }

        /// <summary>
        /// Checks if two rectangles overlap significantly (by more than the given threshold)
        /// </summary>
        private bool RectanglesOverlapSignificantly(Rectangle a, Rectangle b, double threshold)
        {
            // First check if they overlap at all
            if (a.X + a.Width <= b.X || b.X + b.Width <= a.X || a.Y + a.Height <= b.Y || b.Y + b.Height <= a.Y)
                return false;

            // Calculate intersection area
            int intersectLeft = Math.Max(a.X, b.X);
            int intersectRight = Math.Min(a.X + a.Width, b.X + b.Width);
            int intersectTop = Math.Max(a.Y, b.Y);
            int intersectBottom = Math.Min(a.Y + a.Height, b.Y + b.Height);

            int intersectWidth = intersectRight - intersectLeft;
            int intersectHeight = intersectBottom - intersectTop;
            int intersectArea = intersectWidth * intersectHeight;

            // Calculate area of first rectangle
            int areaA = a.Width * a.Height;
            int areaB = b.Width * b.Height;
            int smallerArea = Math.Min(areaA, areaB);

            // Calculate percentage of the smaller rectangle that overlaps
            double overlapPercentage = (double)intersectArea / smallerArea;

            return overlapPercentage > threshold;
        }

        /// <summary>
        /// Convert image file to base64 for API call
        /// </summary>
        private string ConvertImageToBase64(string imagePath)
        {
            try
            {
                byte[] imageBytes = File.ReadAllBytes(imagePath);
                return Convert.ToBase64String(imageBytes);
            }
            catch (Exception ex)
            {
                LogDebug("Error converting image to base64: " + ex.Message);
                throw;
            }
        }

        protected void btnViewResult_Click(object sender, EventArgs e)
        {
            try
            {
                // First, check if we need to update field coordinates from client
                if (!string.IsNullOrEmpty(hdnFieldsData.Value))
                {
                    LogDebug("Updating field coordinates from client before showing results");
                    UpdateFieldCoordinatesFromClient();
                }

                // Now get the most current field coordinates and template URL
                var fieldCoordinates = Session["FieldCoordinates"] as Dictionary<string, FieldData>;
                string templateUrl = Session["TemplateURL"]?.ToString();

                LogDebug($"Showing analysis result with {(fieldCoordinates != null ? fieldCoordinates.Count : 0)} fields");

                if (fieldCoordinates != null && fieldCoordinates.Count > 0 && !string.IsNullOrEmpty(templateUrl))
                {
                    // Add interactive field visualization with draggable fields
                    litAnalysisResult.Text = CreateDraggableFieldVisualization(fieldCoordinates, templateUrl);
                }
                else
                {
                    LogDebug($"Missing data for visualization - Fields: {fieldCoordinates != null}, FieldCount: {fieldCoordinates?.Count}, URL: {!string.IsNullOrEmpty(templateUrl)}");
                    ShowMessage("No analysis results available. Please analyze a template first.", MessageType.Warning);
                    return;
                }

                // Show the analysis popup
                ShowPopup(analysisPopup);
            }
            catch (Exception ex)
            {
                LogDebug($"Error showing results: {ex.Message}\n{ex.StackTrace}");
                ShowMessage("Error showing analysis results: " + ex.Message, MessageType.Error);
            }
        }

        /// <summary>
        /// Creates a field visualization with draggable and resizable fields
        /// </summary>
        private string CreateDraggableFieldVisualization(
            Dictionary<string, FieldData> fieldCoordinates, string templateUrl)
        {
            StringBuilder html = new StringBuilder();

            html.AppendLine("<h3>Field Visualization:</h3>");
            html.AppendLine("<p class='instruction'>Click or drag fields to edit them. Resize by dragging the corner handle.</p>");

            html.AppendLine("<div class='controls'>");
            html.AppendLine("<button type='button' class='btn btn-sm' onclick='addNewField()'>+ Add Field</button>");
            html.AppendLine("<button type='button' class='btn btn-sm btn-secondary' onclick='toggleFieldLabels()'>Toggle Labels</button>");
            html.AppendLine("</div>");

            // Determine orientation and dimensions
            bool isPortrait = Session["IsPortrait"] != null
                ? Convert.ToBoolean(Session["IsPortrait"])
                : true;

            int originalWidth = Convert.ToInt32(Session["ImageWidth"] ?? (isPortrait ? A4_WIDTH : A4_HEIGHT));
            int originalHeight = Convert.ToInt32(Session["ImageHeight"] ?? (isPortrait ? A4_HEIGHT : A4_WIDTH));

            // Set a fixed display width to ensure consistency
            const int displayWidth = 800; // px
            double scale = (double)displayWidth / originalWidth;
            int displayHeight = (int)(originalHeight * scale);

            // Create container with fixed dimensions
            html.AppendLine($@"
<div id='template-container' class='visualization-container' 
     style='width: {displayWidth}px; height: {displayHeight}px; position: relative; margin: 0 auto; border: 1px solid #ddd;' 
     data-scale='{scale.ToString("F6", System.Globalization.CultureInfo.InvariantCulture)}'>
");

            // Resolve template URL
            string resolvedUrl = ResolveUrl(templateUrl);

            // Add template image
            html.AppendLine($"<img src='{resolvedUrl}' id='templateImage' style='width: 100%; height: 100%; object-fit: contain;' />");

            // Add fields
            int index = 0;
            foreach (var field in fieldCoordinates)
            {
                // Scale coordinates to display size
                int scaledX = (int)Math.Max(0, field.Value.Bounds.X * scale);
                int scaledY = (int)Math.Max(0, field.Value.Bounds.Y * scale);
                int scaledWidth = (int)Math.Max(10, field.Value.Bounds.Width * scale);
                int scaledHeight = (int)Math.Max(10, field.Value.Bounds.Height * scale);

                string fieldType = field.Value.Type;
                string fieldName = HttpUtility.HtmlEncode(field.Key);
                bool isRequired = field.Value.IsRequired;

                // Get color based on type
                string fieldColor = GetFieldColor(fieldType);
                string bgColor = GetFieldBackgroundColor(fieldType);

                // Create field marker HTML
                html.AppendLine($@"
<div id='field_{index}' class='field-marker {fieldType.ToLower()}-field' 
     style='position: absolute; left: {scaledX}px; top: {scaledY}px; width: {scaledWidth}px; height: {scaledHeight}px; 
            border: 2px solid {fieldColor}; background-color: {bgColor}; cursor: move; opacity: 0.6;'
     data-original-x='{field.Value.Bounds.X}' data-original-y='{field.Value.Bounds.Y}'
     data-original-width='{field.Value.Bounds.Width}' data-original-height='{field.Value.Bounds.Height}'
     data-field-name='{fieldName}' data-field-type='{fieldType}' data-required='{isRequired.ToString().ToLower()}'
     data-field-index='{index}'>
<div class='field-label' style='position: absolute; top: -20px; background: #fff; border: 1px solid #888; 
        font-size: 12px; padding: 2px 4px;'>{fieldName}</div>
    <div class='resize-handle' style='position: absolute; bottom: 0; right: 0; width: 10px; height: 10px; 
        background-color: white; border: 1px solid #666; cursor: se-resize;'></div>
</div>");

                index++;
            }

            // Close the container
            html.AppendLine("</div>");

            // Add JavaScript for field interaction
            html.AppendLine(@"
<script src='https://code.jquery.com/jquery-3.6.0.min.js'></script>
<script src='https://code.jquery.com/ui/1.13.2/jquery-ui.min.js'></script>
<script type=""text/javascript"">
$(document).ready(function() {
    const scale = parseFloat($('#template-container').data('scale') || 1);
    console.log('Template scale:', scale);
    
    // Make the mini editor draggable
    $('#miniFieldEditor').draggable({
        handle: '.form-group:first',
        containment: 'window'
    });
    
    // Setup field property change listeners
    $('#miniFieldName, #miniFieldType, #miniFieldX, #miniFieldY, #miniFieldWidth, #miniFieldHeight').on('input change', function() {
        miniApplyChanges(false); // Live preview without closing
    });
    
    // Make fields draggable
    $('.field-marker').draggable({
        containment: '#template-container',
        start: function(event, ui) {
            $(this).css('opacity', '0.8');
            openMiniEditor(this, true);
        },
        drag: function(event, ui) {
            // Update original (unscaled) coordinates
            const newX = Math.round(ui.position.left / scale);
            const newY = Math.round(ui.position.top / scale);
            
            // Update mini editor if it's open for this field
            if ($('#miniFieldIndex').val() == $(this).data('field-index')) {
                document.getElementById('miniFieldX').value = newX;
                document.getElementById('miniFieldY').value = newY;
            }
            
            // Update data attributes
            $(this).attr('data-original-x', newX);
            $(this).attr('data-original-y', newY);
        },
        stop: function(event, ui) {
            $(this).css('opacity', '0.6');
            updateFieldsDataFromDOM();
        }
    });
    
    // Setup resize functionality for fields
    $('.field-marker').each(function() {
        let $field = $(this);
        let $handle = $field.find('.resize-handle');
        
        $handle.on('mousedown', function(e) {
            e.stopPropagation();
            e.preventDefault();
            
            let startX = e.pageX;
            let startY = e.pageY;
            let startWidth = $field.width();
            let startHeight = $field.height();
            
            openMiniEditor($field[0], true);
            
            $(document).on('mousemove', function(e) {
                let newWidth = Math.max(20, startWidth + (e.pageX - startX));
                let newHeight = Math.max(10, startHeight + (e.pageY - startY));
                $field.width(newWidth);
                $field.height(newHeight);
                
                // Update original dimensions
                let origWidth = Math.round(newWidth / scale);
                let origHeight = Math.round(newHeight / scale);
                $field.attr('data-original-width', origWidth);
                $field.attr('data-original-height', origHeight);
                
                // Update mini editor
                if ($('#miniFieldIndex').val() == $field.data('field-index')) {
                    document.getElementById('miniFieldWidth').value = origWidth;
                    document.getElementById('miniFieldHeight').value = origHeight;
                }
            });
            
            $(document).on('mouseup', function() {
                $(document).off('mousemove');
                $(document).off('mouseup');
                updateFieldsDataFromDOM();
            });
        });
        
        // Click to select field
        $field.on('click', function(e) {
            e.stopPropagation();
            openMiniEditor(this, false);
        });
    });
    
    // Close mini editor when clicking elsewhere
    $(document).on('click', function(e) {
        if (!$(e.target).closest('.field-marker, #miniFieldEditor').length) {
            miniApplyChanges(true);
        }
    });
});

// Function to open mini editor for a field
function openMiniEditor(fieldElement, keepPosition) {
    const $field = $(fieldElement);
    const fieldName = $field.data('field-name');
    const fieldType = $field.data('field-type');
    const fieldRequired = $field.data('required') === true;
    const fieldIndex = $field.data('field-index');
    
    // Get original coordinates
    const x = parseInt($field.attr('data-original-x'));
    const y = parseInt($field.attr('data-original-y'));
    const width = parseInt($field.attr('data-original-width'));
    const height = parseInt($field.attr('data-original-height'));
    
    // Update mini editor
    $('#miniEditorTitle').text('Edit Field: ' + fieldName);
    $('#miniFieldName').val(fieldName);
    $('#miniFieldType').val(fieldType);
    $('#miniFieldRequired').prop('checked', fieldRequired);
    $('#miniFieldIndex').val(fieldIndex);
    $('#miniFieldX').val(x);
    $('#miniFieldY').val(y);
    $('#miniFieldWidth').val(width);
    $('#miniFieldHeight').val(height);
    
    // Position and show mini editor
    if (!keepPosition || $('#miniFieldEditor').is(':hidden')) {
        const containerOffset = $('#template-container').offset();
        const fieldOffset = $field.offset();
        const fieldWidth = $field.width();
        
        // Position next to field
        let editorLeft = fieldOffset.left + fieldWidth + 10;
        let editorTop = fieldOffset.top;
        
        // Adjust if too close to window edge
        if (editorLeft + $('#miniFieldEditor').outerWidth() > $(window).width()) {
            editorLeft = fieldOffset.left - $('#miniFieldEditor').outerWidth() - 10;
        }
        
        $('#miniFieldEditor').css({
            left: editorLeft + 'px',
            top: editorTop + 'px'
        }).show();
    }
}

// Apply changes from mini editor
function miniApplyChanges(closeEditor) {
    const fieldIndex = $('#miniFieldIndex').val();
    if (fieldIndex === '-1') return;
    
    const $field = $('#field_' + fieldIndex);
    if (!$field.length) return;
    
    const scale = parseFloat($('#template-container').data('scale') || 1);
    const fieldName = $('#miniFieldName').val();
    const fieldType = $('#miniFieldType').val();
    const fieldRequired = $('#miniFieldRequired').is(':checked');
    
    // Get position and size
    const x = parseInt($('#miniFieldX').val() || 0);
    const y = parseInt($('#miniFieldY').val() || 0);
    const width = parseInt($('#miniFieldWidth').val() || 100);
    const height = parseInt($('#miniFieldHeight').val() || 30);
    
    // Update field data
    $field.data('field-name', fieldName);
    $field.attr('data-field-name', fieldName);
    $field.data('field-type', fieldType);
    $field.attr('data-field-type', fieldType);
    $field.data('required', fieldRequired);
    $field.attr('data-required', fieldRequired);
    
    // Update position and size
    $field.css({
        'left': (x * scale) + 'px',
        'top': (y * scale) + 'px',
        'width': (width * scale) + 'px',
        'height': (height * scale) + 'px'
    });
    
    // Update original dimensions
    $field.attr('data-original-x', x);
    $field.attr('data-original-y', y);
    $field.attr('data-original-width', width);
    $field.attr('data-original-height', height);
    
    // Update label text
    $field.find('.field-label').text(fieldName);
    
    // Update fields data for server
    updateFieldsDataFromDOM();
    
    // Close editor if requested
    if (closeEditor) {
        $('#miniFieldEditor').hide();
    }
}

// Delete field from mini editor
function miniDeleteField() {
    const fieldIndex = $('#miniFieldIndex').val();
    if (fieldIndex === '-1') return;
    
    if (confirm('Are you sure you want to delete this field?')) {
        // Remove field from DOM
        $('#field_' + fieldIndex).remove();
        
        // Update fields data
        updateFieldsDataFromDOM();
        
        // Close mini editor
        $('#miniFieldEditor').hide();
    }
}

// Close mini editor
function miniCloseEditor() {
    miniApplyChanges(true);
}

// Update fields data in hidden field
function updateFieldsDataFromDOM() {
    try {
        const fieldsData = [];
        
        $('.field-marker').each(function() {
            const $field = $(this);
            
            const fieldName = $field.attr('data-field-name');
            const fieldType = $field.attr('data-field-type') || 'Text';
            const x = parseInt($field.attr('data-original-x'));
            const y = parseInt($field.attr('data-original-y'));
            const width = parseInt($field.attr('data-original-width'));
            const height = parseInt($field.attr('data-original-height'));
            const required = $field.attr('data-required') === 'true';
            
            if (!fieldName || isNaN(x) || isNaN(y) || isNaN(width) || isNaN(height)) {
                console.warn(""Skipping invalid field:"", fieldName);
                return;
            }
            
            fieldsData.push({
                name: fieldName,
                type: fieldType,
                x: x,
                y: y,
                width: width,
                height: height,
                required: required
            });
        });
        
        // Use jQuery selector which is more reliable
        $('#" + hdnFieldsData.ClientID + @"').val(JSON.stringify(fieldsData));
        console.log(""Updated fields data:"", fieldsData);
    } catch (error) {
        console.error(""Error updating fields data:"", error);
    }
}

// Add new field
function addNewField() {
    const scale = parseFloat($('#template-container').data('scale') || 1);
    
    // Calculate center position
    const containerWidth = $('#template-container').width();
    const containerHeight = $('#template-container').height();
    const centerX = containerWidth / 2 - 100;
    const centerY = containerHeight / 2 - 25;
    
    // Original (unscaled) coordinates
    const origX = Math.round(centerX / scale);
    const origY = Math.round(centerY / scale);
    const origWidth = 200;
    const origHeight = 50;
    
    // Get next field index
    const fieldIndex = $('.field-marker').length;
    
    // Create field HTML
    const fieldHtml = `
<div id='field_${fieldIndex}' class='field-marker text-field' 
     style='position: absolute; left: ${centerX}px; top: ${centerY}px; width: 200px; height: 50px; 
            border: 2px solid #00A2E8; background-color: rgba(0, 162, 232, 0.3); cursor: move; opacity: 0.8;'
     data-original-x='${origX}' data-original-y='${origY}'
     data-original-width='${origWidth}' data-original-height='${origHeight}'
     data-field-name='New Field' data-field-type='Text' data-required='false'
     data-field-index='${fieldIndex}'>
    <div class='field-label' style='position: absolute; top: -20px; background: #fff; border: 1px solid #888; 
        font-size: 12px; padding: 2px 4px;'>New Field</div>
    <div class='resize-handle' style='position: absolute; bottom: 0; right: 0; width: 10px; height: 10px; 
        background-color: white; border: 1px solid #666; cursor: se-resize;'></div>
</div>`;
    
    // Add to container
    $('#template-container').append(fieldHtml);
    
    // Initialize the new field
    const $newField = $(`#field_${fieldIndex}`);
    
    // Make draggable
    $newField.draggable({
        containment: '#template-container',
        start: function(event, ui) {
            $(this).css('opacity', '0.8');
            openMiniEditor(this, true);
        },
        drag: function(event, ui) {
            let newX = Math.round(ui.position.left / scale);
            let newY = Math.round(ui.position.top / scale);
            
            if ($('#miniFieldIndex').val() == $(this).data('field-index')) {
                document.getElementById('miniFieldX').value = newX;
                document.getElementById('miniFieldY').value = newY;
            }
            
            $(this).attr('data-original-x', newX);
            $(this).attr('data-original-y', newY);
        },
        stop: function(event, ui) {
            $(this).css('opacity', '0.6');
            updateFieldsDataFromDOM();
        }
    });
    
    // Setup resize handle
    let $handle = $newField.find('.resize-handle');
    $handle.on('mousedown', function(e) {
        e.stopPropagation();
        e.preventDefault();
        
        let startX = e.pageX;
        let startY = e.pageY;
        let startWidth = $newField.width();
        let startHeight = $newField.height();
        
        openMiniEditor($newField[0], true);
        
        $(document).on('mousemove', function(e) {
            let newWidth = Math.max(20, startWidth + (e.pageX - startX));
            let newHeight = Math.max(10, startHeight + (e.pageY - startY));
            $newField.width(newWidth);
            $newField.height(newHeight);
            
            let origWidth = Math.round(newWidth / scale);
            let origHeight = Math.round(newHeight / scale);
            $newField.attr('data-original-width', origWidth);
            $newField.attr('data-original-height', origHeight);
            
            if ($('#miniFieldIndex').val() == $newField.data('field-index')) {
                document.getElementById('miniFieldWidth').value = origWidth;
                document.getElementById('miniFieldHeight').value = origHeight;
            }
        });
        
        $(document).on('mouseup', function() {
            $(document).off('mousemove');
            $(document).off('mouseup');
            updateFieldsDataFromDOM();
        });
    });
    
    // Click to edit
    $newField.on('click', function(e) {
        e.stopPropagation();
        openMiniEditor(this, false);
    });
    
    // Open mini editor for the new field
    openMiniEditor($newField[0], false);
    
    // Update fields data
    updateFieldsDataFromDOM();
}

// Toggle field labels
function toggleFieldLabels() {
    $('.field-label').toggle();
}

// Hide all popups
function hideAllPopups() {
    $('#overlay').fadeOut();
    $('#analysisPopup').fadeOut();
    $('#backgroundChangePopup').fadeOut();
    $('#miniFieldEditor').hide();
    return false;
}
</script>
");

            return html.ToString();
        }

        protected async void btnSaveAnalysis_Click(object sender, EventArgs e)
        {
            try
            {
                // Get the raw field data from hidden field for debugging
                string fieldDataJson = hdnFieldsData.Value;
                LogDebug("Hidden field data before save: " + fieldDataJson);

                // Update field coordinates from client-side
                UpdateFieldCoordinatesFromClient();

                var fieldCoordinates = Session["FieldCoordinates"] as Dictionary<string, FieldData>;
                if (fieldCoordinates == null || fieldCoordinates.Count == 0)
                {
                    ShowMessage("No field data available to save. Please define at least one field.", MessageType.Error);
                    return;
                }

                // Log all fields being saved for debugging
                LogDebug($"Saving {fieldCoordinates.Count} fields:");
                foreach (var field in fieldCoordinates)
                {
                    LogDebug($"Field: {field.Key}, Type: {field.Value.Type}, Bounds: X={field.Value.Bounds.X}, Y={field.Value.Bounds.Y}, Width={field.Value.Bounds.Width}, Height={field.Value.Bounds.Height}");
                }

                bool isPortrait = (bool)Session["IsPortrait"];
                string templateName = Session["TemplateName"]?.ToString() ?? "Untitled Template";
                string templateType = Session["TemplateType"]?.ToString() ?? "Testimonial";
                bool isEditing = Session["IsEditing"] != null && (bool)Session["IsEditing"];
                string templateId = isEditing ? Session["EditingTemplateId"]?.ToString() : await firebaseHelper.GenerateNextTemplateID();

                // Determine which image URL to use
                string cloudinaryUrl;
                if (Session["NewBackgroundPath"] != null)
                {
                    // Upload new background image to Cloudinary
                    string backgroundImagePath = Session["NewBackgroundPath"].ToString();
                    cloudinaryUrl = cloudinaryHelper.UploadFileFromPath(backgroundImagePath);
                    LogDebug("Uploaded new background to Cloudinary: " + cloudinaryUrl);
                }
                else if (isEditing && Session["OriginalTemplateUrl"] != null)
                {
                    // If editing and no new background was uploaded, keep the original URL
                    cloudinaryUrl = Session["OriginalTemplateUrl"].ToString();
                    LogDebug("Using original template URL: " + cloudinaryUrl);
                }
                else
                {
                    // Use the standardized file
                    string backgroundImagePath = Session["StandardizedFilePath"].ToString();
                    cloudinaryUrl = cloudinaryHelper.UploadFileFromPath(backgroundImagePath);
                    LogDebug("Uploaded standardized file to Cloudinary: " + cloudinaryUrl);
                }

                // Update template data in Firebase
                var templateData = new Dictionary<string, object>
                {
                    { "templateId", templateId },
                    { "templateUrl", cloudinaryUrl },
                    { "templateName", templateName },
                    { "uploadedAt", DateTime.UtcNow.ToString("o") },
                    { "createdBy", Session["UserID"]?.ToString() },
                    { "orientation", isPortrait ? "portrait" : "landscape" },
                    { "templateType", templateType },
                    { "imageWidth", isPortrait ? A4_WIDTH : A4_HEIGHT },
                    { "imageHeight", isPortrait ? A4_HEIGHT : A4_WIDTH },
                    { "isActive", true },
                    { "isDeleted", false }
                };

                // Save to Firebase
                await firebaseHelper.StoreTemplateFieldCoordinates(templateId, fieldCoordinates);
                await firebaseHelper.UpdateTemplateData(templateId, templateData);

                // Hide popup and show success message
                HideAllPopups();
                ShowMessage($"Certificate template {(isEditing ? "updated" : "saved")} successfully.", MessageType.Success);

                // Reset UI for a new upload
                btnUploadAnalyze.Visible = true;
                btnViewResult.Visible = false;

                // Clear session variables to prevent confusion with a new template
                Session.Remove("FieldCoordinates");
                Session.Remove("TemplateURL");
                Session.Remove("StandardizedFilePath");
                Session.Remove("NewBackgroundPath");
                Session.Remove("OriginalFilePath");
                Session.Remove("OriginalTemplateUrl");
                Session.Remove("EditingTemplateId");
                Session.Remove("EditingTemplateUrl");
                Session.Remove("IsEditing");

                // Clean up temporary files
                CleanupTemporaryFiles();
            }
            catch (Exception ex)
            {
                LogDebug($"Error saving template: {ex.Message}\n{ex.StackTrace}");
                ShowMessage($"Error saving template: {ex.Message}", MessageType.Error);
            }
        }

        /// <summary>
        /// Updates the field coordinates from client-side data
        /// </summary>
        private void UpdateFieldCoordinatesFromClient()
        {
            try
            {
                // Get the JSON from hidden field
                string fieldsJson = hdnFieldsData.Value;
                if (string.IsNullOrEmpty(fieldsJson))
                {
                    LogDebug("No field data in hidden field - keeping existing fields");
                    return;
                }

                // Log the raw JSON for debugging
                LogDebug("Raw JSON from hidden field: " + fieldsJson);

                try
                {
                    // Parse JSON into objects
                    var fieldsArray = JsonConvert.DeserializeObject<List<dynamic>>(fieldsJson);
                    if (fieldsArray == null || fieldsArray.Count == 0)
                    {
                        LogDebug("Parsed fields array is empty or null - keeping existing fields");
                        return;
                    }

                    LogDebug($"Successfully parsed {fieldsArray.Count} fields from JSON");

                    // Create a new field coordinates dictionary
                    var fieldCoordinates = new Dictionary<string, FieldData>();

                    foreach (var field in fieldsArray)
                    {
                        try
                        {
                            // Extract field properties safely with explicit conversion
                            string name = Convert.ToString(field.name);
                            if (string.IsNullOrEmpty(name))
                            {
                                LogDebug("Skipping field with empty name");
                                continue;
                            }

                            string type = field.type != null ? Convert.ToString(field.type) : "Text";

                            // Parse coordinates with explicit conversions and defaults if parsing fails
                            int x = 0, y = 0, width = 100, height = 30;
                            bool required = false;

                            if (field.x != null) int.TryParse(field.x.ToString(), out x);
                            if (field.y != null) int.TryParse(field.y.ToString(), out y);
                            if (field.width != null) int.TryParse(field.width.ToString(), out width);
                            if (field.height != null) int.TryParse(field.height.ToString(), out height);
                            if (field.required != null) bool.TryParse(field.required.ToString(), out required);

                            // Validate coordinates (ensure they're within reasonable bounds)
                            x = Math.Max(0, x);
                            y = Math.Max(0, y);
                            width = Math.Max(20, width);
                            height = Math.Max(10, height);

                            // Create field data object
                            var fieldData = new FieldData
                            {
                                Type = type,
                                Label = name,
                                Bounds = new Rectangle(x, y, width, height),
                                IsRequired = required
                            };

                            // Add to dictionary
                            fieldCoordinates[name] = fieldData;

                            LogDebug($"Added field: {name} Type:{type} at ({x},{y}) size {width}x{height}");
                        }
                        catch (Exception ex)
                        {
                            LogDebug($"Error processing individual field from JSON: {ex.Message}");
                            // Continue with other fields even if one fails
                        }
                    }

                    // Make sure we actually have fields to save
                    if (fieldCoordinates.Count > 0)
                    {
                        // Save to session state
                        Session["FieldCoordinates"] = fieldCoordinates;
                        LogDebug($"Successfully updated session with {fieldCoordinates.Count} fields");
                    }
                    else
                    {
                        LogDebug("No valid fields found in JSON - keeping existing fields");
                    }
                }
                catch (Exception jsonEx)
                {
                    LogDebug($"Error parsing fields JSON: {jsonEx.Message}");
                    LogDebug($"JSON content: {fieldsJson}");
                }
            }
            catch (Exception ex)
            {
                LogDebug($"Critical error in UpdateFieldCoordinatesFromClient: {ex.Message}\n{ex.StackTrace}");
                // Don't throw - we want to keep existing fields if there's an error
            }
        }

        protected void btnDeleteField_Click(object sender, EventArgs e)
        {
            try
            {
                if (Session["FieldCoordinates"] == null)
                {
                    ShowMessage("No field data available to delete.", MessageType.Error);
                    return;
                }

                // Get the field name
                string fieldName = hdnOriginalFieldName.Value;

                if (string.IsNullOrEmpty(fieldName))
                {
                    ShowMessage("No field selected to delete.", MessageType.Error);
                    return;
                }

                // Update the field coordinates dictionary
                var fieldCoordinates = Session["FieldCoordinates"] as Dictionary<string, FieldData>;

                if (fieldCoordinates.ContainsKey(fieldName))
                {
                    // Remove the field
                    fieldCoordinates.Remove(fieldName);

                    // Show success message
                    ShowMessage($"Field '{fieldName}' deleted successfully.", MessageType.Success);
                }
                else
                {
                    ShowMessage($"Field '{fieldName}' not found.", MessageType.Error);
                }

                // Hide the field editor popup
                HideAllPopups();

                // Show the analysis popup with updated visualization
                btnViewResult_Click(sender, e);
            }
            catch (Exception ex)
            {
                LogDebug($"Error deleting field: {ex.Message}");
                ShowMessage($"Error deleting field: {ex.Message}", MessageType.Error);
            }
        }

        /// <summary>
        /// Handles the upload of a new background image
        /// </summary>
        protected void btnUploadBackground_Click(object sender, EventArgs e)
        {
            try
            {
                if (!fuNewBackground.HasFile)
                {
                    ShowMessage("Please select a new background image to upload.", MessageType.Error);
                    return;
                }

                // First ensure we capture the current field data
                if (!string.IsNullOrEmpty(hdnFieldsData.Value))
                {
                    LogDebug("Updating field coordinates before background change");
                    UpdateFieldCoordinatesFromClient();
                }

                // Store the current field coordinates temporarily
                var fieldsBeforeUpload = Session["FieldCoordinates"] as Dictionary<string, FieldData>;
                if (fieldsBeforeUpload == null)
                {
                    LogDebug("No field coordinates in session before background upload");
                }
                else
                {
                    LogDebug($"Stored {fieldsBeforeUpload.Count} fields before background upload");
                }

                // Ensure the Uploads folder exists
                string uploadsFolder = Server.MapPath("~/Uploads/");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Get selected orientation
                bool isPortrait = (bool)Session["IsPortrait"];

                // Generate a unique filename for the new background
                string newBgFileName = "BG_" + Guid.NewGuid().ToString("N") + Path.GetExtension(fuNewBackground.FileName);
                string newBgFilePath = Path.Combine(uploadsFolder, newBgFileName);

                // Save the uploaded file
                fuNewBackground.SaveAs(newBgFilePath);

                // Resize to A4 dimensions (using the same orientation as the original)
                string standardizedBgFileName = "A4_" + newBgFileName;
                string standardizedBgFilePath = Path.Combine(uploadsFolder, standardizedBgFileName);

                // Resize image to A4 dimensions
                ResizeImageToA4(newBgFilePath, standardizedBgFilePath, isPortrait);

                // Store the standardized file path for later use
                Session["NewBackgroundPath"] = standardizedBgFilePath;

                // Store new template URL but preserve field positions
                Session["TemplateURL"] = $"~/Uploads/{standardizedBgFileName}";

                // Make sure we restore the field coordinates
                if (fieldsBeforeUpload != null)
                {
                    Session["FieldCoordinates"] = fieldsBeforeUpload;
                }

                // Hide the background change popup
                HideAllPopups();

                // Show success message and update the preview
                ShowMessage("New background image uploaded successfully. Your fields have been preserved.", MessageType.Success);

                // Show the analysis popup with updated visualization (now using the new background)
                btnViewResult_Click(sender, e);
            }
            catch (Exception ex)
            {
                LogDebug($"Error uploading new background: {ex.Message}\n{ex.StackTrace}");
                ShowMessage($"Error uploading new background: {ex.Message}", MessageType.Error);
            }
        }

        protected void btnCancelBackground_Click(object sender, EventArgs e)
        {
            // Hide the background change popup
            HideAllPopups();

            // Show the analysis popup again
            ShowPopup(analysisPopup);
        }

        protected void btnCancelFieldEdit_Click(object sender, EventArgs e)
        {
            // Hide the field editor popup
            HideAllPopups();

            // Re-show the analysis popup
            if (Session["AnalysisResult"] != null)
            {
                ShowPopup(analysisPopup);
            }
        }

        /// <summary>
        /// Cleans up temporary files created during the analysis process
        /// </summary>
        private void CleanupTemporaryFiles()
        {
            try
            {
                // Clean up original file
                if (Session["OriginalFilePath"] != null)
                {
                    string filePath = Session["OriginalFilePath"].ToString();
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                        LogDebug("Deleted original file: " + filePath);
                    }
                }

                // Clean up standardized file
                if (Session["StandardizedFilePath"] != null)
                {
                    string filePath = Session["StandardizedFilePath"].ToString();
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                        LogDebug("Deleted standardized file: " + filePath);
                    }
                }

                // Clean up new background file if it exists
                if (Session["NewBackgroundPath"] != null)
                {
                    string filePath = Session["NewBackgroundPath"].ToString();
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                        LogDebug("Deleted new background file: " + filePath);
                    }
                }
            }
            catch (Exception ex)
            {
                LogDebug($"Error cleaning up temporary files: {ex.Message}");
                // Continue silently - this is just cleanup
            }
        }

        /// <summary>
        /// Gets the border color for a field based on its type
        /// </summary>
        private string GetFieldColor(string fieldType)
        {
            switch (fieldType.ToLower())
            {
                case "date":
                    return "#FF7F27"; // Orange
                case "number":
                    return "#73BF44"; // Green
                case "signature":
                    return "#800080"; // Purple
                case "text":
                default:
                    return "#00A2E8"; // Blue
            }
        }

        /// <summary>
        /// Gets the background color for a field based on its type
        /// </summary>
        private string GetFieldBackgroundColor(string fieldType)
        {
            switch (fieldType.ToLower())
            {
                case "date":
                    return "rgba(255, 127, 39, 0.3)"; // Orange
                case "number":
                    return "rgba(115, 191, 68, 0.3)"; // Green
                case "signature":
                    return "rgba(128, 0, 128, 0.3)"; // Purple
                case "text":
                default:
                    return "rgba(0, 162, 232, 0.3)"; // Blue
            }
        }

        protected void btnCloseAnalysis_Click(object sender, EventArgs e)
        {
            // Hide the analysis popup
            HideAllPopups();
        }

        #region Helper Methods

        /// <summary>
        /// Message types for UI alerts
        /// </summary>
        private enum MessageType
        {
            Success,
            Error,
            Warning,
            Info
        }

        /// <summary>
        /// Display a message in the UI
        /// </summary>
        private void ShowMessage(string message, MessageType type)
        {
            messagePanel.Visible = true;
            lblMessage.Text = message;

            // Set appropriate CSS class based on message type
            switch (type)
            {
                case MessageType.Success:
                    messagePanel.CssClass = "message message-success";
                    break;
                case MessageType.Error:
                    messagePanel.CssClass = "message message-error";
                    break;
                case MessageType.Warning:
                    messagePanel.CssClass = "message message-warning";
                    break;
                case MessageType.Info:
                    messagePanel.CssClass = "message message-info";
                    break;
            }
        }

        /// <summary>
        /// Log debug message to trace
        /// </summary>
        private void LogDebug(string message)
        {
            System.Diagnostics.Debug.WriteLine($"[CertificateAnalyzer] {DateTime.Now}: {message}");
        }

        #endregion

        // Add this to your existing enum
        private enum AnalysisStep
        {
            FieldPlacement,
            BackgroundSelection
        }

        protected void btnProceedToBackgroundStep_Click(object sender, EventArgs e)
        {
            try
            {
                // Explicitly read the hidden field value and update the session
                string fieldData = hdnFieldsData.Value;
                LogDebug("Hidden field data before proceeding: " + fieldData);

                // Update field coordinates from client-side
                UpdateFieldCoordinatesFromClient();

                // Log the fields that are saved
                var fields = Session["FieldCoordinates"] as Dictionary<string, FieldData>;
                if (fields != null && fields.Count > 0)
                {
                    LogDebug($"Saved {fields.Count} fields to session before proceeding to background step");
                    foreach (var field in fields)
                    {
                        LogDebug($"Field: {field.Key}, Position: ({field.Value.Bounds.X}, {field.Value.Bounds.Y}), Size: {field.Value.Bounds.Width}x{field.Value.Bounds.Height}");
                    }

                    // Switch to background selection step
                    ShowBackgroundSelectionStep();
                }
                else
                {
                    LogDebug("No fields were saved to session!");
                    ShowMessage("No fields were detected. Please add at least one field before proceeding.", MessageType.Warning);
                }
            }
            catch (Exception ex)
            {
                LogDebug($"Error in btnProceedToBackgroundStep_Click: {ex.Message}");
                ShowMessage("Error processing field data. Please try again.", MessageType.Error);
            }
        }

        private void ShowBackgroundSelectionStep()
        {
            // Hide field placement popup
            analysisPopup.Style["display"] = "none";

            // Show background change popup
            backgroundChangePopup.Style["display"] = "block";
            overlay.Style["display"] = "block";
        }

        protected void btnSkipBackgroundChange_Click(object sender, EventArgs e)
        {
            // Proceed to save the template with the current background
            btnSaveAnalysis_Click(sender, e);
        }
    }
}