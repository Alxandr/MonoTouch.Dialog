using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.Dialog.Utilities;
using System.Threading;

namespace MonoTouch.Dialog
{
	public class DialogCell : UITableViewCell
	{
		private DialogStyle dialogStyle;
		private Element element;
		private RectangleF _oldFrame;
		private int _threshold = 0;
		const int THRESHOLD = 10;

		public virtual Element Element {
			get {
				return element;
			}
			set {
				element = value;
				dialogStyle = element.Style;
				SetNeedsLayout ();
			}
		}
		public DialogCell (UITableViewCellStyle style, NSString reuseIdentifier)
			: base(style, reuseIdentifier)
		{
		}

		public override void LayoutSubviews ()
		{
			var frame = Frame;
			if (frame == _oldFrame) {
				if(_threshold >= THRESHOLD) return;
				_threshold++;
			} else {
				_oldFrame = frame;
			}

			dialogStyle.StyleCell (this, element);
			base.LayoutSubviews ();
			dialogStyle.LayoutCell (this, element);
		}

		public override void SetNeedsLayout ()
		{
			_oldFrame = default(RectangleF);
			_threshold = 0;
			base.SetNeedsLayout ();
		}
	}
	
	public class DialogStyle : IDisposable
	{
		static DialogStyle _default = new DialogStyle ();
		public static DialogStyle Default { get { return _default; } }
		
		protected DialogStyle ()
		{
			
		}
		
		public virtual void StyleTable (UITableView tableView)
		{
			// Do nothing in the default Style
		}
		
		public virtual void StyleCaption (UILabel label, Element element)
		{
			// Do nothing in the default Style
		}
		
		public virtual void LayoutCell (UITableViewCell cell, Element element)
		{
			// Do nothing in the default Style
		}
		
		public virtual void StyleCell (UITableViewCell cell, Element element)
		{
			// Do nothing in the default Style
		}
		
		public virtual UIView SetupView (UITableView tableView)
		{
			// Simply insert tableView as view in default Style
			return tableView;
		}
		
		public void ComputeEntryAlignment (RootElement root)
		{
			var result = ComputeEntryAlignment (root.Sections, root.TableView);
			for (int i = 0; i < root.Sections.Count; i++)
				root.Sections [i].EntryAlignment = result [i];
		}
		
		static UIFont font = UIFont.BoldSystemFontOfSize (17);
		protected virtual SizeF[] ComputeEntryAlignment (List<Section> sections, UITableView tv)
		{
			return ComputeEntryAlignment (sections, tv, font);
		}
		
		protected virtual SizeF[] ComputeEntryAlignment (List<Section> sections, UITableView tv, UIFont font)
		{
			SizeF[] ret = new SizeF[sections.Count];
			int i = 0;
			foreach (var s in sections) {
				// If all EntryElements have a null Caption, align UITextField with the Caption
				// offset of normal cells (at 10px).
				SizeF max = new SizeF (10, tv.StringSize ("M", DialogStyle.font).Height);
				var height = max.Height;
				
				foreach (var e in s.Elements) {
					var ce = e as ICaptionElement;
					if (ce == null)
						continue;
					
					if (e.Caption != null) {
						var size = tv.StringSize (e.Caption, font);
						if (size.Width > max.Width)
							max = size;
					}
				}
				max.Height = height;
				ret [i++] = max;
			}
			return ret;
		}
		
		public enum RowType {
			Top,
			Mid,
			Bot,
			TopBot
		}
		
		protected RowType GetRowType (Element element)
		{
			var parent = element.Parent as Section;
			if (parent.Elements [0] == element && parent.Elements [parent.Elements.Count - 1] == element)
				return RowType.TopBot;
			else if (parent.Elements [0] == element)
				return RowType.Top;
			else if (parent.Elements [parent.Elements.Count - 1] == element)
				return RowType.Bot;
			return RowType.Mid;
		}
		
		#region IDisposable implementation
		public void Dispose ()
		{
			
		}
		#endregion
		
		private class DialogBackgroundView : UIView
		{
			private UIView backgroundView;
			private UITableView tableView;
			private NSObject openObserver;
			private NSObject closeObserver;
			private NSObject closedObserver;
			private float keyboardHeight;
			
			public DialogBackgroundView (UIView backgroundView, UITableView tableView)
			{
				AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
				this.backgroundView = backgroundView;
				this.tableView = tableView;
				AddSubview (backgroundView);
				AddSubview (tableView);
				
				openObserver = NSNotificationCenter.DefaultCenter.AddObserver (UIKeyboard.WillShowNotification, ResizeKeyboardEvent);
				closeObserver = NSNotificationCenter.DefaultCenter.AddObserver (UIKeyboard.WillHideNotification, ResizeKeyboardEvent);
				closedObserver = NSNotificationCenter.DefaultCenter.AddObserver (UIKeyboard.DidHideNotification, KeyboardDidHide);

				tableView.SeparatorColor = UIColor.Clear;
				tableView.BackgroundView = null;
			}
			
			~DialogBackgroundView ()
			{
				NSNotificationCenter.DefaultCenter.RemoveObserver (openObserver);
				NSNotificationCenter.DefaultCenter.RemoveObserver (closeObserver);
				NSNotificationCenter.DefaultCenter.RemoveObserver (closedObserver);
			}
			
			private void KeyboardDidHide (NSNotification notification)
			{
				keyboardHeight = 0;
				SetNeedsLayout ();
			}
			
			public override void LayoutSubviews ()
			{
				base.LayoutSubviews ();
				tableView.Frame = new RectangleF (tableView.Frame.Location, new SizeF(Frame.Width, Frame.Height - keyboardHeight));
			}
			
			private void ResizeKeyboardEvent (NSNotification notification)
			{	
				var currentFrame = UIKeyboard.FrameBeginFromNotification (notification);
				var endFrame = UIKeyboard.FrameEndFromNotification (notification);
				var duration = UIKeyboard.AnimationDurationFromNotification (notification);
				var curve = UIKeyboard.AnimationCurveFromNotification (notification);
				currentFrame.Intersect (UIScreen.MainScreen.Bounds);
				endFrame.Intersect (UIScreen.MainScreen.Bounds);
				
				if (endFrame.Height == UIScreen.MainScreen.Bounds.Height) {
					endFrame = new RectangleF (
						endFrame.Y, endFrame.X,
						endFrame.Height, endFrame.Width
					);
				}
				
				keyboardHeight = Math.Max (endFrame.Height, 0);
				if (keyboardHeight > 0 && Superview != null) {
					var superFrame = Superview.Frame;
					var topOffset = superFrame.Top;
					var height = UIScreen.MainScreen.Bounds.Height - topOffset - keyboardHeight;
					keyboardHeight = Math.Min (keyboardHeight, tableView.Frame.Height - height);
				}

				
				if (Window == null) {
					tableView.Frame = new RectangleF (tableView.Frame.Location, 
				                                  new SizeF (tableView.Frame.Width, Frame.Height - keyboardHeight));
				} else {
					UIView.BeginAnimations ("KeyboardFit");
					UIView.SetAnimationCurve ((UIViewAnimationCurve)curve);
					UIView.SetAnimationDuration (duration);
					tableView.Frame = new RectangleF (tableView.Frame.Location, 
				                                  new SizeF (tableView.Frame.Width, Frame.Height - keyboardHeight));
					UIView.CommitAnimations ();
				}
			}
		}
		
		public static UIView TableWithBackgroundView (UITableView tableView, UIView backgroundView)
		{
			return new DialogBackgroundView (backgroundView, tableView);
		}
	}
	
	public class ContactsDialogStyle : DialogStyle
	{
		static UIFont font = UIFont.BoldSystemFontOfSize(12);
		float width = 50f;
		
		public override void StyleTable (UITableView tableView)
		{
			// TODO: Calculate captions width
		}
		
		public override void StyleCaption (UILabel label, Element element)
		{
			label.Font = font;
			label.TextColor = UIColor.FromRGB (81, 102, 145);
			if (element is ICaptionElement)
				label.TextAlignment = UITextAlignment.Right;
		}
		
		public override void LayoutCell (UITableViewCell cell, Element element)
		{
			var frame = cell.TextLabel.Frame;
			frame.Width = width;
			Element.RemoveTag (cell, 2);
			if (element is ICaptionElement) {
				cell.TextLabel.Frame = frame;
				cell.TextLabel.TextAlignment = UITextAlignment.Right;
				UIView border = new UIView (new RectangleF (cell.ContentView.Frame.X + width + 5, cell.ContentView.Frame.Top, 1, cell.ContentView.Frame.Height));
				border.BackgroundColor = UIColor.LightGray;
				border.Tag = 2;
				cell.ContentView.AddSubview (border);
			}
		}
		
		
		protected override SizeF[] ComputeEntryAlignment (List<Section> sections, UITableView tv)
		{
			var ret = ComputeEntryAlignment (sections, tv, font);
			var widest = Widest (ret);
			width = widest;
			for (int i = 0; i < ret.Length; i++)
				ret [i].Width = widest;
			return ret;
		}
		
		private float Widest (IEnumerable<SizeF> sizes)
		{
			var max = -1f;
			foreach (var s in sizes) {
				max = Math.Max (s.Width, max);
			}
			return max;
		}
	}
}