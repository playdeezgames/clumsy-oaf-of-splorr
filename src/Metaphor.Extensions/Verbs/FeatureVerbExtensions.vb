Imports System.Runtime.CompilerServices
Imports Metaphor.Persistence

Public Module FeatureVerbExtensions
    Private Delegate Function CanPerformHandler(verb As IVerb, feature As IFeature, actor As ICharacter) As Boolean
    Private Delegate Sub PerformHandler(verb As IVerb, feature As IFeature, actor As ICharacter)
#Region "Can Perform"
    Private ReadOnly canPerformTable As New Dictionary(Of String, CanPerformHandler) From
        {
            {VerbSubtypes.ENTER, AddressOf CanEnter},
            {VerbSubtypes.SLEEP, AddressOf CanSleep},
            {VerbSubtypes.ADD_FLOUR, CanAddIngredient(Counters.FLOUR, True, False)},
            {VerbSubtypes.ADD_SUGAR, CanAddIngredient(Counters.SUGAR, True, False)},
            {VerbSubtypes.ADD_VANILLA, CanAddIngredient(Counters.VANILLA, False, False)},
            {VerbSubtypes.ADD_BUTTER, CanAddIngredient(Counters.BUTTER, False, False)},
            {VerbSubtypes.ADD_EGG, CanAddIngredient(Counters.EGG, False, False)},
            {VerbSubtypes.ADD_MILK, CanAddIngredient(Counters.MILK, True, False)},
            {VerbSubtypes.ADD_BAKING_POWDER, CanAddIngredient(Counters.BAKING_POWDER, False, True)},
            {VerbSubtypes.ADD_BAKING_SODA, CanAddIngredient(Counters.BAKING_SODA, False, True)},
            {VerbSubtypes.ADD_SALT, CanAddIngredient(Counters.SALT, False, True)},
            {VerbSubtypes.EMPTY_MIXING_BOWL, AddressOf CanEmptyMixingBowl},
            {VerbSubtypes.TURN_ON, AddressOf CanTurnOn},
            {VerbSubtypes.TURN_OFF, AddressOf CanTurnOff},
            {VerbSubtypes.OPEN_DOOR, AddressOf CanOpenDoor},
            {VerbSubtypes.BAKE_CAKE, AddressOf CanBakeCake},
            {VerbSubtypes.PUT_CAKE_PAN_IN, AddressOf CanPutCakePanIn},
            {VerbSubtypes.TAKE_CAKE_PAN_OUT, AddressOf CanTakeCakePanOut},
            {VerbSubtypes.CLOSE_DOOR, AddressOf CanCloseDoor},
            {VerbSubtypes.BUY_SUPPLIES, AddressOf CanBuySupplies},
            {VerbSubtypes.BUY_CAKE_BOARD, AddressOf CanBuyCakeBoard}
        }

    Private Function CanBuyCakeBoard(verb As IVerb, feature As IFeature, actor As ICharacter) As Boolean
        Return actor.GetDimension(Dimensions.JOOLS) >= 1.0
    End Function

    Private Function CanBuySupplies(verb As IVerb, feature As IFeature, actor As ICharacter) As Boolean
        Return actor.GetDimension(Dimensions.JOOLS) >= verb.GetDimension(Dimensions.JOOLS)
    End Function

    Private Function CanTakeCakePanOut(verb As IVerb, feature As IFeature, actor As ICharacter) As Boolean
        Return feature.EntitySubtype = FeatureSubtypes.OVEN AndAlso
            feature.HasTag(Tags.OPEN) AndAlso
            feature.Inventory.HasItemOfSubtype(ItemSubtypes.CAKE_PAN)
    End Function

    Private Function CanPutCakePanIn(verb As IVerb, feature As IFeature, actor As ICharacter) As Boolean
        Return feature.EntitySubtype = FeatureSubtypes.OVEN AndAlso
            feature.HasTag(Tags.OPEN) AndAlso
            Not feature.Inventory.HasItemOfSubtype(ItemSubtypes.CAKE_PAN) AndAlso
            actor.Inventory.HasItemOfSubtype(ItemSubtypes.CAKE_PAN)
    End Function

    Private Function CanBakeCake(verb As IVerb, feature As IFeature, actor As ICharacter) As Boolean
        Return feature.EntitySubtype = FeatureSubtypes.OVEN AndAlso
            feature.HasTag(Tags.ON) AndAlso
            Not feature.HasTag(Tags.OPEN) AndAlso
            feature.Inventory.HasItemOfSubtype(ItemSubtypes.CAKE_PAN) AndAlso
            Not feature.Inventory.GetItemsOfSubtype(ItemSubtypes.CAKE_PAN).Any(Function(x) x.IsDimensionMinimum(Dimensions.BATTER))
    End Function

    Private Function CanCloseDoor(verb As IVerb, feature As IFeature, actor As ICharacter) As Boolean
        Return feature.EntitySubtype = FeatureSubtypes.OVEN AndAlso feature.HasTag(Tags.OPEN)
    End Function

    Private Function CanOpenDoor(verb As IVerb, feature As IFeature, actor As ICharacter) As Boolean
        Return feature.EntitySubtype = FeatureSubtypes.OVEN AndAlso Not feature.HasTag(Tags.OPEN)
    End Function

    Private Function CanTurnOff(verb As IVerb, feature As IFeature, actor As ICharacter) As Boolean
        Return feature.EntitySubtype = FeatureSubtypes.OVEN AndAlso feature.HasTag(Tags.[ON])
    End Function

    Private Function CanTurnOn(verb As IVerb, feature As IFeature, actor As ICharacter) As Boolean
        Return feature.EntitySubtype = FeatureSubtypes.OVEN AndAlso Not feature.HasTag(Tags.[ON])
    End Function

    Private Function CanAddIngredient(counterId As String, needsMeasuringCup As Boolean, needsMeasuringSpoons As Boolean) As CanPerformHandler
        Return Function(verb As IVerb, feature As IFeature, actor As ICharacter)
                   Return Not actor.IsDead AndAlso
                        Not feature.IsCounterMinimum(counterId) AndAlso
                        (Not needsMeasuringCup OrElse actor.Inventory.HasItemOfSubtype(ItemSubtypes.MEASURING_CUP)) AndAlso
                        (Not needsMeasuringSpoons OrElse actor.Inventory.HasItemOfSubtype(ItemSubtypes.MEASURING_SPOONS)) AndAlso
                        actor.Inventory.Items.Any(Function(x) x.EntitySubtype = ItemSubtypes.MIXING_BOWL AndAlso Not x.IsCounterMaximum(counterId))
               End Function
    End Function

    Private Function CanEmptyMixingBowl(verb As IVerb, feature As IFeature, actor As ICharacter) As Boolean
        Dim mixingBowl = actor.Inventory.Items.FirstOrDefault(Function(x) x.EntitySubtype = ItemSubtypes.MIXING_BOWL)
        Return Not actor.IsDead AndAlso If(mixingBowl?.IsEmpty(), False)
    End Function

    Private Function CanSleep(verb As IVerb, feature As IFeature, actor As ICharacter) As Boolean
        Return Not actor.IsDead AndAlso actor.GetCounter(Counters.ENERGY) < actor.GetCounterMaximum(Counters.ENERGY) \ 2
    End Function

    Private Function CanEnter(verb As IVerb, feature As IFeature, actor As ICharacter) As Boolean
        Return Not actor.IsDead
    End Function

    <Extension>
    Public Function CanPerform(verb As IVerb, feature As IFeature, actor As ICharacter) As Boolean
        Dim handler As CanPerformHandler = Nothing
        If canPerformTable.TryGetValue(verb.EntitySubtype, handler) Then
            Return handler.Invoke(verb, feature, actor)
        End If
        Return True
    End Function
#End Region
#Region "Perform"
    Private ReadOnly performTable As New Dictionary(Of String, PerformHandler) From
        {
            {VerbSubtypes.ENTER, AddressOf HandleEnter},
            {VerbSubtypes.SLEEP, AddressOf HandleSleep},
            {VerbSubtypes.ADD_FLOUR, HandleAddIngredient(Counters.FLOUR, "flour")},
            {VerbSubtypes.ADD_SUGAR, HandleAddIngredient(Counters.SUGAR, "sugar")},
            {VerbSubtypes.ADD_BAKING_POWDER, HandleAddIngredient(Counters.BAKING_POWDER, "baking powder")},
            {VerbSubtypes.ADD_BAKING_SODA, HandleAddIngredient(Counters.BAKING_SODA, "baking soda")},
            {VerbSubtypes.ADD_SALT, HandleAddIngredient(Counters.SALT, "salt")},
            {VerbSubtypes.ADD_BUTTER, HandleAddIngredient(Counters.BUTTER, "butter")},
            {VerbSubtypes.ADD_EGG, HandleAddIngredient(Counters.EGG, "egg")},
            {VerbSubtypes.ADD_MILK, HandleAddIngredient(Counters.MILK, "milk")},
            {VerbSubtypes.ADD_VANILLA, HandleAddIngredient(Counters.VANILLA, "vanilla")},
            {VerbSubtypes.EMPTY_MIXING_BOWL, AddressOf HandleEmptyMixingBowl},
            {VerbSubtypes.TURN_ON, AddressOf HandleTurnOn},
            {VerbSubtypes.TURN_OFF, AddressOf HandleTurnOff},
            {VerbSubtypes.OPEN_DOOR, AddressOf HandleOpenDoor},
            {VerbSubtypes.PUT_CAKE_PAN_IN, AddressOf HandlePutCakePanIn},
            {VerbSubtypes.TAKE_CAKE_PAN_OUT, AddressOf HandleTakeCakePanOut},
            {VerbSubtypes.BAKE_CAKE, AddressOf HandleBakeCake},
            {VerbSubtypes.CLOSE_DOOR, AddressOf HandleCloseDoor},
            {VerbSubtypes.BUY_SUPPLIES, AddressOf HandleBuySupplies},
            {VerbSubtypes.BUY_CAKE_BOARD, AddressOf HandleBuyCakeBoard}
        }

    Private Sub HandleBuyCakeBoard(verb As IVerb, feature As IFeature, actor As ICharacter)
        Dim jools = 1.0
        Dim cakeBoard = actor.Inventory.CreateCakeboard()
        actor.AddMessage($"{actor.Name} buys {cakeBoard.Name} for {jools:F2} jools.")
        actor.ChangeDimension(Dimensions.JOOLS, -jools)
        actor.AddMessage($"{actor.Name} now has {actor.GetDimension(Dimensions.JOOLS):f2} jools.")
    End Sub

    Private Sub HandleBuySupplies(verb As IVerb, feature As IFeature, actor As ICharacter)
        Dim jools = verb.GetDimension(Dimensions.JOOLS)
        actor.AddMessage($"{actor.Name} spends {jools:f2} jools")
        actor.ChangeDimension(Dimensions.JOOLS, -jools)
        actor.AddMessage($"{actor.Name} now has {actor.GetDimension(Dimensions.JOOLS):f2} jools")
        Dim targetFeature As IFeature = actor.Location.Features.Single(Function(x) x.EntitySubtype = verb.GetMetadata(Metadatas.FEATURE_SUBTYPE))
        targetFeature.MaximumCounter(verb.GetMetadata(Metadatas.COUNTER_ID))
        targetFeature.Describe()
    End Sub

    Private Sub HandleBakeCake(verb As IVerb, feature As IFeature, actor As ICharacter)
        actor.AddMessage($"{actor.Name} waits until the cake is done.")
        actor.DoBiology(1)
        Dim cakePan = feature.Inventory.GetItemsOfSubtype(ItemSubtypes.CAKE_PAN).Single()
        cakePan.MinimizeDimension(Dimensions.BATTER)
        cakePan.SetTag(Tags.CAKE)
    End Sub

    Private Sub HandleTakeCakePanOut(verb As IVerb, feature As IFeature, actor As ICharacter)
        Dim cakePan = feature.Inventory.GetItemsOfSubtype(ItemSubtypes.CAKE_PAN).First
        actor.AddMessage($"{actor.Name} takes {cakePan.Name} from {feature.Name}.")
        cakePan.Container = actor.Inventory
    End Sub

    Private Sub HandlePutCakePanIn(verb As IVerb, feature As IFeature, actor As ICharacter)
        Dim cakePan = actor.Inventory.GetItemsOfSubtype(ItemSubtypes.CAKE_PAN).First
        actor.AddMessage($"{actor.Name} puts {cakePan.Name} into {feature.Name}.")
        cakePan.Container = feature.Inventory
    End Sub

    Private Sub HandleCloseDoor(verb As IVerb, feature As IFeature, actor As ICharacter)
        feature.ClearTag(Tags.OPEN)
    End Sub

    Private Sub HandleOpenDoor(verb As IVerb, feature As IFeature, actor As ICharacter)
        feature.SetTag(Tags.OPEN)
    End Sub

    Private Sub HandleTurnOff(verb As IVerb, feature As IFeature, actor As ICharacter)
        feature.ClearTag(Tags.ON)
    End Sub

    Private Sub HandleTurnOn(verb As IVerb, feature As IFeature, actor As ICharacter)
        feature.SetTag(Tags.ON)
    End Sub

    Private Function HandleAddIngredient(counterId As String, counterName As String) As PerformHandler
        Return Sub(verb As IVerb, feature As IFeature, actor As ICharacter)
                   Dim cup = actor.Inventory.Items.First(Function(x) x.EntitySubtype = ItemSubtypes.MEASURING_CUP)
                   Dim bowl = actor.Inventory.Items.First(Function(x) x.EntitySubtype = ItemSubtypes.MIXING_BOWL)
                   actor.AddMessage($"{actor.Name} uses {cup.Name} to move 1 {counterName} from {feature.Name} to {bowl.Name}.")
                   feature.ChangeCounter(counterId, -1)
                   actor.AddMessage($"{feature.Name} now has {feature.GetCounter(counterId)} {counterName}.")
                   bowl.ChangeCounter(counterId, 1)
                   actor.AddMessage($"{bowl.Name} now has {bowl.GetCounter(counterId)} {counterName}.")
               End Sub
    End Function

    Private Sub HandleEmptyMixingBowl(verb As IVerb, feature As IFeature, actor As ICharacter)
        Dim mixingBowl = actor.Inventory.Items.Single(Function(x) x.EntitySubtype = ItemSubtypes.MIXING_BOWL)
        mixingBowl.Empty()
        actor.AddMessage($"{actor.Name} empties {mixingBowl.Name} into {feature.Name}.")
    End Sub
    Private Sub HandleSleep(verb As IVerb, feature As IFeature, actor As ICharacter)
        actor.AddMessage($"{actor.Name} sleeps.")
        Dim energy = actor.GetCounterCapacity(Counters.ENERGY)
        actor.AddMessage($"{actor.Name} gains {energy} energy.")
        actor.ChangeCounter(Counters.ENERGY, energy)
        actor.AddMessage($"{actor.Name} now has {actor.GetCounterStatistic(Counters.ENERGY)} energy.")
    End Sub

    Private Sub HandleEnter(verb As IVerb, feature As IFeature, actor As ICharacter)
        actor.AddMessage($"{actor.Name} goes through {feature.Name}.")
        actor.DoBiology()
        actor.Location = feature.GetDestination()
        actor.Look()
    End Sub
    <Extension>
    Sub Perform(verb As IVerb, feature As IFeature, actor As ICharacter)
        Dim handler As PerformHandler = Nothing
        If performTable.TryGetValue(verb.EntitySubtype, handler) Then
            handler.Invoke(verb, feature, actor)
            Return
        End If
    End Sub
#End Region
End Module
