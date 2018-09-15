using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OauthIdentityApp.Models.System
{
    public class FunctionViewModel
    {
        public string ID { set; get; }

        [Required]
        [MaxLength(50)]
        public string Name { set; get; }

        [Required]
        [MaxLength(256)]
        public string URL { set; get; }

        public int DisplayOrder { set; get; }

        public string ParentId { set; get; }

        public FunctionViewModel Parent { set; get; }

        public ICollection<FunctionViewModel> ChildFunctions { set; get; }


        public bool Status { set; get; }

        public bool Menu { set; get; }

        public string IconCss { get; set; }
        public bool expanded { set; get; }
    }
}