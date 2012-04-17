using System;
using MonoTouch.Dialog;
using MonoTouch.UIKit;

namespace Sample
{
	public partial class AppDelegate
	{
		public void DemoContactStyle ()
		{
			var root = CreateRoot ();
			DialogViewController dvc = new DialogViewController (new StyleClass (), root, true);
			
			navigation.PushViewController (dvc, true);
		}
		
		class StyleClass : ContactsDialogStyle
		{
			public override MonoTouch.UIKit.UIView SetupView (MonoTouch.UIKit.UITableView tableView)
			{
				var image = UIImage.FromFile ("background_style.png");
				var view = new UIImageView (image);
				tableView.BackgroundColor = UIColor.Clear;
				return DialogStyle.TableWithBackgroundView (tableView, view);
			}
		}
	}
}

