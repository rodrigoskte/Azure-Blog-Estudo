﻿using System.ComponentModel.DataAnnotations;

namespace Blog.ViewModels.Categories
{
    public class EditorCategoryViewModel
    {
        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(40, MinimumLength = 3, ErrorMessage = "Este campo precisa ter entre 3 e 40 caracteres")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Slug é obrigatório")]
        public string Slug { get; set; }
    }
}
