using MudBlazor;
using MudBlazor.Utilities;

namespace Torch2WebUI.Services
{
    public class ThemeService
    {
        public MudTheme? CurrentTheme { get; private set; }

        public bool IsDarkMode { get; set; } = true;

        public ThemeService()
        {
            CreateTheme();
        }

        public void CreateTheme()
        {
            //Will need to load the theme from saved config later
            CurrentTheme = new MudTheme()
            {
                PaletteLight = _defaultlightPalette,
                PaletteDark = _defaultdarkPalette,
                LayoutProperties = new LayoutProperties()
                {
                    DefaultBorderRadius = "6px"
                }
            };


        }

        private readonly PaletteLight _defaultlightPalette = new()
        {
            Black = "#110e2d",
            AppbarText = "#424242",
            AppbarBackground = "rgba(255,255,255,0.8)",
            DrawerBackground = "#ffffff",
            GrayLight = "#e8e8e8",
            GrayLighter = "#f9f9f9",
        };

        private readonly PaletteDark _defaultdarkPalette = new()
        {
            Primary = "#e1824bff",
            Surface = "#1e1e2d",
            Background = "#1e1e28ff",
            BackgroundGray = "#151521",
            AppbarText = "#92929f",
            AppbarBackground = "rgba(26,26,39,0.8)",
            DrawerBackground = "#1a1a27",
            ActionDefault = "#74718e",
            ActionDisabled = "#9999994d",
            ActionDisabledBackground = "#605f6d4d",
            TextPrimary = "#b2b0bf",
            TextSecondary = "#92929f",
            TextDisabled = "#ffffff33",
            DrawerIcon = "#92929f",
            DrawerText = "#92929f",
            GrayLight = "#2a2833",
            GrayLighter = "#1e1e2d",
            Info = "#4a86ff",
            Success = "#3dcb6c",
            Warning = "#ffb545",
            Error = "#ff3f5f",
            LinesDefault = "#33323e",
            TableLines = "#33323e",
            Divider = "#292838",
            OverlayLight = "#1e1e2d80",
        };



    }
}
