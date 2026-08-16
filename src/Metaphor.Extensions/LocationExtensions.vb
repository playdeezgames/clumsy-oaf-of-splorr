Imports System.Runtime.CompilerServices
Imports Metaphor.Persistence

Public Module LocationExtensions
    <Extension>
    Public Sub CreateN00b(location As ILocation, name As String, initializer As CharacterInitializer)
        location.CreateCharacter(CharacterSubtypes.N00B, name, initializer)
    End Sub
    <Extension>
    Public Sub CreateDoor(location As ILocation, name As String, initializer As FeatureInitializer)
        location.CreateFeature(FeatureSubtypes.DOOR, name, initializer)
    End Sub
    <Extension>
    Public Sub CreateCupboard(location As ILocation, initializer As FeatureInitializer)
        location.CreateFeature(FeatureSubtypes.CUPBOARD, "Cupboard", initializer)
    End Sub
    <Extension>
    Public Sub CreateDryPantry(location As ILocation, initializer As FeatureInitializer)
        location.CreateFeature(FeatureSubtypes.DRY_PANTRY, "Dry Pantry", initializer)
    End Sub
    <Extension>
    Public Sub CreateBin(location As ILocation, initializer As FeatureInitializer)
        location.CreateFeature(FeatureSubtypes.BIN, "Bin", initializer)
    End Sub
    <Extension>
    Public Sub CreateRefrigerator(location As ILocation, initializer As FeatureInitializer)
        location.CreateFeature(FeatureSubtypes.REFRIGERATOR, "Refrigerator", initializer)
    End Sub
    <Extension>
    Public Sub CreateOven(location As ILocation, initializer As FeatureInitializer)
        location.CreateFeature(FeatureSubtypes.OVEN, "Oven", initializer)
    End Sub
    <Extension>
    Public Sub CreateSupplyDrawer(location As ILocation, initializer As FeatureInitializer)
        location.CreateFeature(FeatureSubtypes.SUPPLY_DRAWER, "Supply Drawer", initializer)
    End Sub
    <Extension>
    Public Sub CreateBed(location As ILocation, initializer As FeatureInitializer)
        location.CreateFeature(FeatureSubtypes.BED, "Bed", initializer)
    End Sub
    <Extension>
    Public Sub CreateComputer(location As ILocation, initializer As FeatureInitializer)
        location.CreateFeature(FeatureSubtypes.COMPUTER, "Computer", initializer)
    End Sub
    <Extension>
    Public Sub CreateRecipeBox(location As ILocation, initializer As FeatureInitializer)
        location.CreateFeature(FeatureSubtypes.RECIPE_BOX, "Recipe Box", initializer)
    End Sub
End Module
