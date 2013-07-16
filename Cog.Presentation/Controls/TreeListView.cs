using System.Windows.Controls;
using System.Windows;

namespace SIL.Cog.Presentation.Controls
{
    public class TreeListView : TreeView
    {
        static TreeListView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TreeListView), new FrameworkPropertyMetadata(typeof(TreeListView)));

            //ItemsPanelTemplate template = new ItemsPanelTemplate(new FrameworkElementFactory(typeof(VirtualizingStackPanel)));
            //template.Seal();
            //ItemsControl.ItemsPanelProperty.OverrideMetadata(typeof(TreeListView), new FrameworkPropertyMetadata(template));
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TreeListViewItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is TreeListViewItem;
        }

	    #region Public Properties

        /// <summary> GridViewColumn List</summary>
        public GridViewColumnCollection Columns
        {
            get
            {
                if (_columns == null)
                {
                    _columns = new GridViewColumnCollection();
                }

                return _columns;
            }
        }

        private GridViewColumnCollection _columns;

        #endregion
    }
}
