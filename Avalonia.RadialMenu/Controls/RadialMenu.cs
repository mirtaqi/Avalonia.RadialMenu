using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Reactive;
using Avalonia.Styling;

namespace Avalonia.RadialMenu.Controls;

public class RadialMenu : ContentControl
{
    public RadialMenu()
    {
        IsOpenProperty.Changed.Subscribe(new AnonymousObserver<AvaloniaPropertyChangedEventArgs<bool>>(args =>
        {
            if (!args.NewValue.Value)
            {
                SelectedItem=null;
            }
        }));
    }
    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<RadialMenu, bool>(nameof(IsOpen), false);

    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set
        {
            SetValue(IsOpenProperty, value);
        }
    }

    public static readonly StyledProperty<bool> HalfShiftedItemsProperty =
        AvaloniaProperty.Register<RadialMenu, bool>(nameof(HalfShiftedItems), false);

    public bool HalfShiftedItems
    {
        get => GetValue(HalfShiftedItemsProperty);
        set => SetValue(HalfShiftedItemsProperty, value);
    }

    public static readonly StyledProperty<RadialMenuCentralItem?> CentralItemProperty =
        AvaloniaProperty.Register<RadialMenu, RadialMenuCentralItem?>(nameof(CentralItem), null);

    public RadialMenuCentralItem? CentralItem
    {
        get => GetValue(CentralItemProperty);
        set => SetValue(CentralItemProperty, value);
    }

    public static readonly StyledProperty<ObservableCollection<RadialMenuItem>?> MenuContentProperty =
        AvaloniaProperty.Register<RadialMenu, ObservableCollection<RadialMenuItem>?>(nameof(MenuContent),
            new ObservableCollection<RadialMenuItem>());

    public ObservableCollection<RadialMenuItem>? MenuContent
    {
        get => GetValue(MenuContentProperty);
        set => SetValue(MenuContentProperty, value);
    }

    public static readonly StyledProperty<double> LevelRadiusProperty =
        AvaloniaProperty.Register<RadialMenu, double>(nameof(LevelRadius), 100d);

    public double LevelRadius
    {
        get => GetValue(LevelRadiusProperty);
        set => SetValue(LevelRadiusProperty, value);
    }

    public static readonly StyledProperty<RadialMenuItem?> SelectedItemProperty =
        AvaloniaProperty.Register<RadialMenuItem, RadialMenuItem?>(nameof(SelectedItem), null);

    public RadialMenuItem? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set
        {
            SetValue(SelectedItemProperty, value); 
            this.InvalidateMeasure();
            this.InvalidateArrange();
        }
    }

    static RadialMenu()
    {
    }

    private int GetMaxLevel(ObservableCollection<RadialMenuItem> menuItems,int level)
    {
        if (menuItems.Count == 0)
        {
            return level;
        }

        return menuItems.Select(u => GetMaxLevel(u.SubMenuItems, level+1)).Max();
    }
    protected override Size MeasureOverride(Size availableSize)
    {
        //return base.MeasureOverride(availableSize);
        var maxLevel = GetMaxLevel(MenuContent, 0);
        var size = 30 + LevelRadius* maxLevel;
        return new Size(size*2, size*2);
    }

    protected override void ArrangeCore(Rect finalRect)
    {
        //var levels = MenuContent.Select(m => m.MenuLevel).Distinct().ToArray();
        //var levelCount = levels.Length;
        //foreach (var level in levels)
        //{
        //    var menuItems = MenuContent.Where(m => m.MenuLevel == level).ToArray();
        //    for (int i = 0; i < menuItems.Length; i++)
        //    {
        //        menuItems[i].Index = i;
        //        menuItems[i].Count = menuItems.Length;
        //        menuItems[i].HalfShifted = HalfShiftedItems;
        //    }
        //}
        //for (int i = 0, count = MenuContent!.Count; i < count; i++)
        //{
        //    MenuContent[i].Index = i;
        //    MenuContent[i].Count = count;
        //    MenuContent[i].HalfShifted = HalfShiftedItems;
        //}
        ArrangeMenuItems(null,MenuContent,0);
        base.ArrangeCore(finalRect);
    }

    private void ArrangeMenuItems(RadialMenuItem? parent, ObservableCollection<RadialMenuItem> menuItems,int level)
    {
        //var menuLevelToShow = 0;
        //if (SelectedItem is not null)
        //{
        //    menuLevelToShow = SelectedItem.MenuLevel + 1;
        //}
        
        var count = menuItems.Count;
        for (int i = 0; i < count; i++)
        {
            menuItems[i].RadialMenu = this;
            menuItems[i].IsVisible = menuItems[i].MenuLevel == 0 || menuItems[i].IsChildOf(SelectedItem);

            menuItems[i].ParentMenuItem=parent;
            

            menuItems[i].MenuLevel = level;
            menuItems[i].Index = i;

            menuItems[i].Count = count;
            menuItems[i].HalfShifted = HalfShiftedItems;
            ArrangeMenuItems(menuItems[i],menuItems[i].SubMenuItems,level+1);
            menuItems[i].InvalidateArrange();
        }
    }
}
