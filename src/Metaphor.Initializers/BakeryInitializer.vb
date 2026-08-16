Imports Metaphor.Extensions
Imports Metaphor.Persistence

Friend Module BakeryInitializer
    Friend Function Initialize(
                              context As IInitializationContext,
                              blueRoom As ILocation) As LocationInitializer
        Return Sub(bakery)
                   bakery.CreateDoor("Door to Blue Room", InitializeBlueRoomDoor(context, blueRoom))
                   bakery.CreateCupboard(InitializeCupboard(context))
                   bakery.CreateDryPantry(InitializeDryPantry(context))
                   bakery.CreateBin(InitializeBin(context))
                   bakery.CreateRefrigerator(InitializeRefrigerator(context))
                   bakery.CreateOven(InitializeOven(context))
                   bakery.CreateSupplyDrawer(InitializeSupplyDrawer(context))
                   bakery.CreateComputer(InitializeComputer(context))
                   bakery.CreateRecipeBox(InitializeRecipeBox(context))
               End Sub
    End Function

    Private Function InitializeRecipeBox(context As IInitializationContext) As FeatureInitializer
        Return Sub(recipeBox)
                   recipeBox.Inventory.CreateRecipeCard()
               End Sub
    End Function

    Private Function InitializeComputer(context As IInitializationContext) As FeatureInitializer
        Return Sub(computer)
                   computer.AddPrices()
               End Sub
    End Function

    Private Function InitializeSupplyDrawer(context As IInitializationContext) As FeatureInitializer
        Return Sub(drawer)
                   drawer.Inventory.CreateCakeboard()
                   drawer.Inventory.CreateCakeboard()
                   drawer.Inventory.CreateCakeboard()
               End Sub
    End Function

    Private Function InitializeOven(context As IInitializationContext) As FeatureInitializer
        Return Sub(oven)
                   oven.CreateVerb(VerbSubtypes.TURN_ON, "Turn On")
                   oven.CreateVerb(VerbSubtypes.TURN_OFF, "Turn Off")
                   oven.CreateVerb(VerbSubtypes.OPEN_DOOR, "Open Door")
                   oven.CreateVerb(VerbSubtypes.CLOSE_DOOR, "Close Door")
                   oven.CreateVerb(VerbSubtypes.PUT_CAKE_PAN_IN, "Put Cake Pan In")
                   oven.CreateVerb(VerbSubtypes.TAKE_CAKE_PAN_OUT, "Take Cake Pan Out")
                   oven.CreateVerb(VerbSubtypes.BAKE_CAKE, "Bake Cake")
                   oven.SetTag(Tags.SUPPRESS_ITEMS)
               End Sub
    End Function

    Private Function InitializeRefrigerator(context As IInitializationContext) As FeatureInitializer
        Return Sub(refrigerator)
                   refrigerator.InitializeCounter(Counters.EGG, 30, 0, 60)
                   refrigerator.InitializeCounter(Counters.MILK, 50, 0, 100)
                   refrigerator.InitializeCounter(Counters.BUTTER, 25, 0, 50)
                   refrigerator.CreateVerb(VerbSubtypes.ADD_EGG, "Add Egg to Mixing Bowl")
                   refrigerator.CreateVerb(VerbSubtypes.ADD_BUTTER, "Add Butter to Mixing Bowl")
                   refrigerator.CreateVerb(VerbSubtypes.ADD_MILK, "Add Milk to Mixing Bowl")
               End Sub
    End Function

    Private Function InitializeBin(context As IInitializationContext) As FeatureInitializer
        Return Sub(bin)
                   bin.CreateVerb(VerbSubtypes.EMPTY_MIXING_BOWL, "Empty Mixing Bowl")
               End Sub
    End Function

    Private Function InitializeDryPantry(context As IInitializationContext) As FeatureInitializer
        Return Sub(pantry)
                   pantry.InitializeCounter(Counters.FLOUR, 50, 0, 100)
                   pantry.InitializeCounter(Counters.SUGAR, 50, 0, 100)
                   pantry.InitializeCounter(Counters.BAKING_POWDER, 25, 0, 50)
                   pantry.InitializeCounter(Counters.BAKING_SODA, 25, 0, 50)
                   pantry.InitializeCounter(Counters.VANILLA, 25, 0, 50)
                   pantry.InitializeCounter(Counters.SALT, 250, 0, 500)
                   pantry.CreateVerb(VerbSubtypes.ADD_FLOUR, "Add Flour to Mixing Bowl")
                   pantry.CreateVerb(VerbSubtypes.ADD_SUGAR, "Add Sugar to Mixing Bowl")
                   pantry.CreateVerb(VerbSubtypes.ADD_VANILLA, "Add Vanilla to Mixing Bowl")
                   pantry.CreateVerb(VerbSubtypes.ADD_SALT, "Add Salt to Mixing Bowl")
                   pantry.CreateVerb(VerbSubtypes.ADD_BAKING_POWDER, "Add Baking Powder to Mixing Bowl")
                   pantry.CreateVerb(VerbSubtypes.ADD_BAKING_SODA, "Add Baking Soda to Mixing Bowl")
               End Sub
    End Function

    Private Function InitializeCupboard(context As IInitializationContext) As FeatureInitializer
        Return Sub(cupboard)
                   cupboard.Inventory.CreateMixingBowl()
                   cupboard.Inventory.CreateMeasuringCup()
                   cupboard.Inventory.CreateMeasuringSpoons()
                   cupboard.Inventory.CreateWoodenSpoon()
                   cupboard.Inventory.CreateCakePan()
               End Sub
    End Function

    Private Function InitializeBlueRoomDoor(
                                           context As IInitializationContext,
                                           blueRoom As ILocation) As FeatureInitializer
        Return Sub(feature)
                   feature.SetDestination(blueRoom)
                   feature.CreateEnterVerb()
               End Sub
    End Function
End Module
