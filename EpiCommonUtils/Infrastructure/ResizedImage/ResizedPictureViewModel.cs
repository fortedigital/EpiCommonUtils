namespace Forte.EpiCommonUtils.Infrastructure.ResizedImage
{
    public class ResizedPictureViewModel
    {
            public ResizedPictureViewModel(string url, string alt)
            {
                this.Url = url;
                this.Alt = alt;
            }
        
            public string Url { get; }
            public string Alt { get; }
        }
    }
