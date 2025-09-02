using Microsoft.AspNetCore.Razor.TagHelpers;
using Noundry.UI.Core;

namespace Noundry.UI.Components;

[HtmlTargetElement("noundry-toast-container")]
public class ToastContainerTagHelper : NoundryTagHelperBase
{
    /// <summary>
    /// Position of the toast container (top-right, top-left, bottom-right, bottom-left, top-center, bottom-center)
    /// </summary>
    public string Position { get; set; } = "top-right";

    /// <summary>
    /// Maximum width of the toast container
    /// </summary>
    public string MaxWidth { get; set; } = "max-w-sm";

    /// <summary>
    /// Z-index for the toast container
    /// </summary>
    public string ZIndex { get; set; } = "z-50";

    /// <summary>
    /// Whether to enable sound for toast notifications
    /// </summary>
    public bool EnableSound { get; set; } = false;

    /// <summary>
    /// Default duration for toast notifications in milliseconds
    /// </summary>
    public int DefaultDuration { get; set; } = 3000;

    /// <summary>
    /// Additional CSS classes for the container
    /// </summary>
    public string? CssClass { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var content = await output.GetChildContentAsync();

        output.TagName = "div";

        // Generate unique ID for this toast container
        var containerId = GenerateUniqueId("toast-container");

        // Set positioning classes based on position
        var positionClasses = GetPositionClasses();

        var classes = new List<string>
        {
            "fixed", positionClasses, ZIndex, "flex", "flex-col", "gap-3", "w-full", MaxWidth
        };

        if (!string.IsNullOrEmpty(CssClass))
        {
            classes.Add(CssClass);
        }

        AddCssClasses(output, classes.ToArray());

        // Set Alpine.js attributes
        SetAlpineAttribute(output, "data", "toastComponent()");
        SetAlpineAttribute(output, "init", "init()");
        output.Attributes.SetAttribute("id", containerId);
        SetAlpineAttribute(output, "@toast.window", "add($event.detail)");

        // Generate the toast container HTML with embedded JavaScript
        var toastHtml = GenerateToastContainerHtml();
        var javascript = GenerateToastJavaScript();

        output.Content.SetHtmlContent($@"
            {toastHtml}
            
            <script>
            {javascript}
            </script>
        ");
    }

    private string GetPositionClasses()
    {
        return Position.ToLower() switch
        {
            "top-left" => "top-4 left-4",
            "top-center" => "top-4 left-1/2 transform -translate-x-1/2",
            "top-right" => "top-4 right-4",
            "bottom-left" => "bottom-4 left-4",
            "bottom-center" => "bottom-4 left-1/2 transform -translate-x-1/2",
            "bottom-right" => "bottom-4 right-4",
            _ => "top-4 right-4"
        };
    }

    private string GenerateToastContainerHtml()
    {
        return @"
            <template x-for=""(toast, index) in toasts"" :key=""toast.id"">
              <div 
                :class=""{
                  'border-l-4 border-green-500 bg-gradient-to-r from-green-50 to-white': toast.type === 'success',
                  'border-l-4 border-red-500 bg-gradient-to-r from-red-50 to-white': toast.type === 'error',
                  'border-l-4 border-blue-500 bg-gradient-to-r from-blue-50 to-white': toast.type === 'info',
                  'border-l-4 border-yellow-500 bg-gradient-to-r from-yellow-50 to-white': toast.type === 'warning',
                  'border-l-4 border-gray-500 bg-gradient-to-r from-gray-50 to-white': toast.type === 'default'
                }""
                class=""flex items-center p-4 mb-1 text-gray-800 rounded-lg shadow-lg transform transition-all duration-500 ease-out relative overflow-hidden backdrop-blur-sm bg-opacity-95""
                :style=""`transition-delay: ${index * 100}ms`""
                x-transition:enter=""translate-x-full opacity-0 scale-95""
                x-transition:enter-start=""translate-x-full opacity-0 scale-95""
                x-transition:enter-end=""translate-x-0 opacity-100 scale-100""
                x-transition:leave=""translate-x-0 opacity-100 scale-100""
                x-transition:leave-start=""translate-x-0 opacity-100 scale-100""
                x-transition:leave-end=""translate-x-full opacity-0 scale-95""
              >
                <!-- Toast progress bar -->
                <div 
                  class=""absolute bottom-0 left-0 h-1 bg-opacity-40""
                  :class=""{
                    'bg-green-500': toast.type === 'success',
                    'bg-red-500': toast.type === 'error',
                    'bg-blue-500': toast.type === 'info',
                    'bg-yellow-500': toast.type === 'warning',
                    'bg-gray-500': toast.type === 'default'
                  }""
                  :style=""`width: 100%; animation: shrink ${toast.duration}ms linear forwards;`""
                ></div>
                
                <!-- Toast icon -->
                <div class=""flex-shrink-0 mr-3"">
                  <template x-if=""toast.type === 'success'"">
                    <svg class=""w-6 h-6 text-green-500"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                      <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z""></path>
                    </svg>
                  </template>
                  <template x-if=""toast.type === 'error'"">
                    <svg class=""w-6 h-6 text-red-500"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                      <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z""></path>
                    </svg>
                  </template>
                  <template x-if=""toast.type === 'info'"">
                    <svg class=""w-6 h-6 text-blue-500"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                      <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z""></path>
                    </svg>
                  </template>
                  <template x-if=""toast.type === 'warning'"">
                    <svg class=""w-6 h-6 text-yellow-500"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                      <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z""></path>
                    </svg>
                  </template>
                  <template x-if=""toast.type === 'default'"">
                    <svg class=""w-6 h-6 text-gray-500"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                      <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M8 12h.01M12 12h.01M16 12h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z""></path>
                    </svg>
                  </template>
                </div>
                
                <!-- Toast content -->
                <div class=""flex-1"">
                  <div class=""font-medium"" x-text=""toast.message""></div>
                </div>
                
                <!-- Close button -->
                <button 
                  @click=""remove(toast.id)"" 
                  class=""ml-3 p-1 rounded-full hover:bg-gray-200 focus:outline-none focus:ring-2 focus:ring-gray-300 transition-colors duration-200""
                  aria-label=""Close toast""
                >
                  <svg class=""w-4 h-4 text-gray-500"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                    <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M6 18L18 6M6 6l12 12""></path>
                  </svg>
                </button>
              </div>
            </template>";
    }

    private string GenerateToastJavaScript()
    {
        return $@"
// Ensure we only add the CSS animations once
if (!document.getElementById('noundry-toast-animations')) {{
  const style = document.createElement('style');
  style.id = 'noundry-toast-animations';
  style.textContent = `
    @keyframes shrink {{
      0% {{ width: 100%; }}
      100% {{ width: 0%; }}
    }}
    @keyframes bounce-in {{
      0% {{ transform: scale(0.8); opacity: 0; }}
      70% {{ transform: scale(1.05); opacity: 1; }}
      100% {{ transform: scale(1); opacity: 1; }}
    }}
    .toast-bounce-in {{
      animation: bounce-in 0.5s ease-out forwards;
    }}
  `;
  document.head.appendChild(style);
}}

// Toast component function for Alpine.js
function toastComponent() {{
  return {{
    toasts: [],
    init() {{
      // Component is ready
    }},
    add(toast) {{
      // Generate a unique ID for the toast
      const id = Date.now().toString() + Math.random().toString(36).substr(2, 9);

      // Add the toast to the array
      this.toasts.push({{
        id,
        message: toast.message,
        type: toast.type || 'default',
        duration: toast.duration || {DefaultDuration},
      }});

      {(EnableSound ? @"
      // Play sound if provided
      if (toast.sound) {
        const audio = new Audio(toast.sound);
        audio.volume = 0.5;
        audio.play().catch((e) => console.log('Audio play failed:', e));
      }" : "")}

      // Remove the toast after the duration
      setTimeout(() => {{
        this.remove(id);
      }}, toast.duration || {DefaultDuration});
    }},
    remove(id) {{
      this.toasts = this.toasts.filter((toast) => toast.id !== id);
    }},
  }};
}}

// Global toast API - only initialize if not already present
if (!window.toast) {{
  window.toast = {{
    show(message, type = 'default', duration = {DefaultDuration}, options = {{}}) {{
      window.dispatchEvent(
        new CustomEvent('toast', {{
          detail: {{
            message,
            type,
            duration,
            {(EnableSound ? "sound: options.sound || null," : "")}
          }},
        }})
      );
    }},
    
    // Convenience methods
    success(message, duration = {DefaultDuration}) {{
      this.show(message, 'success', duration);
    }},
    error(message, duration = {DefaultDuration}) {{
      this.show(message, 'error', duration);
    }},
    info(message, duration = {DefaultDuration}) {{
      this.show(message, 'info', duration);
    }},
    warning(message, duration = {DefaultDuration}) {{
      this.show(message, 'warning', duration);
    }},
  }};
}}";
    }
}