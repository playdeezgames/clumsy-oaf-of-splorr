Imports System.Runtime.CompilerServices
Imports Metaphor.Persistence

Public Module InventoryExtensions
#Region "Mixing Bowl"
    <Extension>
    Sub CreateMixingBowl(inventory As IInventory)
        inventory.CreateItem(ItemSubtypes.MIXING_BOWL, "Mixing Bowl", AddressOf InitializeMixingBowl)
    End Sub
    Private Sub InitializeMixingBowl(item As IItem)
        item.InitializeCounter(Counters.FLOUR, 0, 0, 5)
        item.InitializeCounter(Counters.SUGAR, 0, 0, 5)
        item.InitializeCounter(Counters.BAKING_POWDER, 0, 0, 5)
        item.InitializeCounter(Counters.BAKING_SODA, 0, 0, 5)
        item.InitializeCounter(Counters.SALT, 0, 0, 5)
        item.InitializeCounter(Counters.VANILLA, 0, 0, 5)
        item.InitializeCounter(Counters.BUTTER, 0, 0, 5)
        item.InitializeCounter(Counters.MILK, 0, 0, 5)
        item.InitializeCounter(Counters.EGG, 0, 0, 5)
        item.CreateVerb(VerbSubtypes.MIX, "Mix")
        item.InitializeDimension(Dimensions.BATTER, 0.0, 0.0, Double.MaxValue)
        item.InitializeDimension(Dimensions.GLOP, 0.0, 0.0, Double.MaxValue)
    End Sub
#End Region
#Region "Measuring Cup"
    <Extension>
    Sub CreateMeasuringCup(inventory As IInventory)
        inventory.CreateItem(ItemSubtypes.MEASURING_CUP, "Measuring Cup")
    End Sub
#End Region
#Region "Measuring Spoons"
    <Extension>
    Sub CreateMeasuringSpoons(inventory As IInventory)
        inventory.CreateItem(ItemSubtypes.MEASURING_SPOONS, "Measuring Spoons")
    End Sub
#End Region
#Region "Wooden Spoon"
    <Extension>
    Sub CreateWoodenSpoon(inventory As IInventory)
        inventory.CreateItem(ItemSubtypes.WOODEN_SPOON, "Wooden Spoon")
    End Sub
#End Region
#Region "Cake Pan"
    <Extension>
    Sub CreateCakePan(inventory As IInventory)
        inventory.CreateItem(ItemSubtypes.CAKE_PAN, "Cake Pan", AddressOf InitializeCakePan)
    End Sub
    Private Sub InitializeCakePan(item As IItem)
        item.InitializeDimension(Dimensions.BATTER, 0.0, 0.0, Double.MaxValue)
        item.CreateVerb(VerbSubtypes.POUR_BATTER, "Pour Batter")
    End Sub
#End Region
#Region "Cake Board"
    <Extension>
    Public Function CreateCakeboard(inventory As IInventory) As IItem
        Return inventory.CreateItem(
            ItemSubtypes.CAKE_BOARD,
            "Cake Board",
            Sub(cakeBoard)
                cakeBoard.InitializeCounter(Counters.LAYERS, 0, 0, 5)
                cakeBoard.CreateVerb(VerbSubtypes.UNMOLD_CAKE, "Unmold Cake")
                cakeBoard.CreateVerb(VerbSubtypes.SELL_CAKE, "Sell Cake")
            End Sub)
    End Function
#End Region
#Region "Recipe Card"
    <Extension>
    Public Sub CreateRecipeCard(inventory As IInventory)
        inventory.CreateItem(ItemSubtypes.RECIPE_CARD, "Recipe Card")
    End Sub
#End Region
End Module
