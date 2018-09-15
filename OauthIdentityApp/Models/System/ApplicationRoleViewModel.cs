using System.ComponentModel.DataAnnotations;

namespace OauthIdentityApp.Models.System
{
    public class ApplicationRoleViewModel
    {
        public string Id { set; get; }

        [Required(ErrorMessage = "Bạn phải nhập tên")]
        public string Name { set; get; }

        public string Description { set; get; }
    }
}