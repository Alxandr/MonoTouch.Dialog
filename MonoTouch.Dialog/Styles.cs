using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.Dialog.Utilities;

namespace MonoTouch.Dialog
{
	public class DialogCell : UITableViewCell
	{
		private DialogStyle dialogStyle;
		private Element element;
		public Element Element {
			get {
				return element;
			}
			set {
				element = value;
				dialogStyle = element.Style;
			}
		}
		public DialogCell (UITableViewCellStyle style, NSString reuseIdentifier)
			: base(style, reuseIdentifier)
		{
		}
		
		public override void LayoutSubviews ()
		{
			base.LayoutSubviews ();
			dialogStyle.LayoutCell (this, element);
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
		
		#region IDisposable implementation
		public void Dispose ()
		{
			
		}
		#endregion
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