using System;
using MonoTouch.Dialog;

namespace Sample
{
	public partial class AppDelegate
	{
		public void DemoContactStyle ()
		{
			var root = CreateRoot ();
			DialogViewController dvc = new DialogViewController (new ContactsDialogStyle(), root, true);
			
			navigation.PushViewController (dvc, true);
		}
	}
}

