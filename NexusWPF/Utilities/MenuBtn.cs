﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NexusWPF.Utilities
{
    public class MenuBtn : RadioButton
    {
        static MenuBtn()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MenuBtn), new FrameworkPropertyMetadata(typeof(MenuBtn)));
        }
    }
}
