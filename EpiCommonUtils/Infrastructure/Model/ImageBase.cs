using System.ComponentModel.DataAnnotations;
using EPiServer.Core;
using EPiServer.DataAnnotations;

namespace Forte.EpiCommonUtils.Infrastructure.Model
{
    public class ImageBase : ImageData
    {
        [CultureSpecific]
        [Editable(true)]
        [Display(
            Name = "Alternate text",
            Description = "Description of the image")]
        public virtual string Description { get; set; }
    }
}
