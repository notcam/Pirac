﻿using System.ComponentModel;
using Conventional.Conventions;

namespace Pirac.Conventions
{
    internal class ViewModelConvention : Convention
    {
        public ViewModelConvention()
        {
            Must.HaveNameEndWith("ViewModel").BeAClass();

            Should.BeAConcreteClass().Implement<INotifyPropertyChanged>();

            BaseName = t => t.Name.Substring(0, t.Name.Length - 9);

            Variants.HaveBaseNameAndEndWith("ViewModel");
        }
    }
}