using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Reactive;
using Avalonia.Styling;

namespace Avalonia.RadialMenu.Controls;

public class RadialMenu : ContentControl
{
    private readonly AnonymousObserver<AvaloniaPropertyChangedEventArgs<double>> _canvasLeftTopObserver;
    private bool _disableCanvasObserver = false;
    private bool _isCanvasPositionChanged = true;
    private void OnCanvasLeftTopChanged(AvaloniaPropertyChangedEventArgs<double> obj)
    {

        if (_disableCanvasObserver) return;
      
      
        _isCanvasPositionChanged = true;
        
        if (obj.Sender == this)
        {
        
            if (obj.Property == Canvas.LeftProperty)
            {
                AlignCenterX();

            }
            else if (obj.Property == Canvas.TopProperty)
            {
                AlignCenterY();
            }
            _disableCanvasObserver = false;
        }
        
    }

    public RadialMenu()
    {
        _canvasLeftTopObserver =
            new AnonymousObserver<AvaloniaPropertyChangedEventArgs<double>>(OnCanvasLeftTopChanged);
        Canvas.LeftProperty.Changed.Subscribe(_canvasLeftTopObserver);
        Canvas.TopProperty.Changed.Subscribe(_canvasLeftTopObserver);

        IsOpenProperty.Changed.Subscribe(new AnonymousObserver<AvaloniaPropertyChangedEventArgs<bool>>(args =>
        {
            if (!args.NewValue.Value)
            {
                SelectedItem = null;
            }
            else
            {
              AlignCenterToCanvasPosition();
            }
        }));
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        AlignCenterToCanvasPosition();
    }

    private double _lastCanvasX=0, _lastCanvasY=0;
    private void AlignCenterToCanvasPosition()
    {
        AlignCenterX();
        AlignCenterY();
    }

    private void AlignCenterX()
    {
        if (this.IsSet(Canvas.LeftProperty) && this.IsLoaded && _isCanvasPositionChanged && this.Bounds.Width > 0 && this.Bounds.Height > 0)
        {
            _disableCanvasObserver = true;

            var left = (double)this.GetValue(Canvas.LeftProperty);

            if (Math.Abs(_lastCanvasX - left) < 1 )
                return;
            var level1Radius = this.Bounds.Width / 2d;

            left -= level1Radius;

            this.SetValue(Canvas.LeftProperty, left);
            _lastCanvasX = left;
            


            _isCanvasPositionChanged = false;
            _disableCanvasObserver = false;
        }
    }
    private void AlignCenterY()
    {
        if (this.IsSet(Canvas.TopProperty) && this.IsLoaded && _isCanvasPositionChanged && this.Bounds.Width > 0 && this.Bounds.Height > 0)
        {
            _disableCanvasObserver = true;

            var top = (double)this.GetValue(Canvas.TopProperty);

            if (Math.Abs(_lastCanvasY - top) < 1)
                return;
            var level1Radius = this.Bounds.Width / 2d;

             
            top -= level1Radius;
            this.SetValue(Canvas.TopProperty, top);
            _lastCanvasY = top;


            _isCanvasPositionChanged = false;
            _disableCanvasObserver = false;
        }
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

    private int GetMaxLevel(ObservableCollection<RadialMenuItem> menuItems, int level)
    {
        if (menuItems.Count == 0)
        {
            return level;
        }

        return menuItems.Select(u => GetMaxLevel(u.SubMenuItems, level + 1)).Max();
    }

    private int _lastMaxLevel = -1;
    protected override Size MeasureOverride(Size availableSize)
    {
        //return base.MeasureOverride(availableSize);
        var centerMenuRadius = 30d;
        if (CentralItem is not null)
        {
            centerMenuRadius = CentralItem.Bounds.Width / 2;
        }
        var maxLevel = GetMaxLevel(MenuContent, 0);
        
        var size = centerMenuRadius + LevelRadius * maxLevel;

        if (_lastMaxLevel != maxLevel)
        {
            this.InvalidateMeasure();
        }
        _lastMaxLevel= maxLevel;
        return new Size(size * 2, size * 2);
    }


    public double CalculateLevel1Radius()
    {
        var centerMenuRadius = 30d;
        if (CentralItem is not null)
        {
            centerMenuRadius = CentralItem.Bounds.Width / 2;
        }

        return centerMenuRadius + LevelRadius;
    }
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

    }
    
    protected override void ArrangeCore(Rect finalRect)
    {
        Debug.WriteLine("Radial menu arrange core");
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
        ArrangeMenuItems(null, MenuContent, 0);
        base.ArrangeCore(finalRect);
    }

    private void ArrangeMenuItems(RadialMenuItem? parent, ObservableCollection<RadialMenuItem> menuItems, int level)
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
            menuItems[i].IsVisible = menuItems[i].MenuLevel == 0 || (SelectedItem is not null && menuItems[i].IsSelfOrChildOf(SelectedItem.GetSelfAndAncestors().ToArray()));

            menuItems[i].ParentMenuItem = parent;


            menuItems[i].MenuLevel = level;
            menuItems[i].Index = i;

            menuItems[i].Count = count;
            menuItems[i].HalfShifted = HalfShiftedItems;
            ArrangeMenuItems(menuItems[i], menuItems[i].SubMenuItems, level + 1);
            //menuItems[i].InvalidateArrange();
        }
    }


}
