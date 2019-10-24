using Avalonia;
using Avalonia.Markup.Xaml;

namespace TinyIV.App
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
   }
}