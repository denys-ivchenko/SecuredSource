﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telesyk.SecuredSource
{
	public interface IEnablingState
	{
		void SetEnablingState(bool disable);
	}
}
