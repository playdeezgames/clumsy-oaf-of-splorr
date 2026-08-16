Imports System.Runtime.CompilerServices
Imports Metaphor.Persistence

Public Module ItemVerbExtensions
    Private Delegate Function CanPerformHandler(verb As IVerb, item As IItem, actor As ICharacter) As Boolean
    Private Delegate Sub PerformHandler(verb As IVerb, item As IItem, actor As ICharacter)
#Region "Can Perform"
    Private ReadOnly canPerformTable As New Dictionary(Of String, CanPerformHandler) From
        {
            {VerbSubtypes.MIX, AddressOf CanMix},
            {VerbSubtypes.POUR_BATTER, AddressOf CanPourBatter},
            {VerbSubtypes.UNMOLD_CAKE, AddressOf CanUnmoldCake},
            {VerbSubtypes.SELL_CAKE, AddressOf CanSellCake}
        }

    Private Function CanSellCake(verb As IVerb, item As IItem, actor As ICharacter) As Boolean
        Return item.EntitySubtype = ItemSubtypes.CAKE_BOARD AndAlso
            Not item.IsCounterMinimum(Counters.LAYERS)
    End Function

    Private Function CanUnmoldCake(verb As IVerb, item As IItem, actor As ICharacter) As Boolean
        Dim cakePan = actor.Inventory.GetItemsOfSubtype(ItemSubtypes.CAKE_PAN).FirstOrDefault
        Return item.EntitySubtype = ItemSubtypes.CAKE_BOARD AndAlso
            Not item.IsCounterMaximum(Counters.LAYERS) AndAlso
            cakePan IsNot Nothing AndAlso
            cakePan.HasTag(Tags.CAKE)
    End Function

    Private Function CanPourBatter(verb As IVerb, item As IItem, actor As ICharacter) As Boolean
        Dim mixingBowl = actor.Inventory.GetItemsOfSubtype(ItemSubtypes.MIXING_BOWL).FirstOrDefault()
        Return item.EntitySubtype = ItemSubtypes.CAKE_PAN AndAlso
            mixingBowl IsNot Nothing AndAlso
            mixingBowl.HasBatter()
    End Function

    Private Function CanMix(verb As IVerb, item As IItem, actor As ICharacter) As Boolean
        Return item.EntitySubtype = ItemSubtypes.MIXING_BOWL AndAlso
            Not item.IsEmpty() AndAlso
            actor.Inventory.HasItemOfSubtype(ItemSubtypes.WOODEN_SPOON)
    End Function

    <Extension>
    Public Function CanPerform(verb As IVerb, item As IItem, actor As ICharacter) As Boolean
        Dim handler As CanPerformHandler = Nothing
        If canPerformTable.TryGetValue(verb.EntitySubtype, handler) Then
            Return handler.Invoke(verb, item, actor)
        End If
        Return True
    End Function
#End Region
#Region "Perform"
    Private ReadOnly performTable As New Dictionary(Of String, PerformHandler) From
        {
            {VerbSubtypes.MIX, AddressOf HandleMix},
            {VerbSubtypes.POUR_BATTER, AddressOf HandlePourBatter},
            {VerbSubtypes.UNMOLD_CAKE, AddressOf HandleUnmoldCake},
            {VerbSubtypes.SELL_CAKE, AddressOf HandleSellCake}
        }

    Private Sub HandleSellCake(verb As IVerb, item As IItem, actor As ICharacter)
        Dim layers = item.GetCounter(Counters.LAYERS)
        Dim jools = Grimoire.JOOLS_PER_LAYER * layers
        actor.AddMessage($"{actor.Name} sells {layers} layer cake for {jools:F2} jools.")
        actor.ChangeDimension(Dimensions.JOOLS, jools)
        actor.AddMessage($"{actor.Name} now has {actor.GetDimension(Dimensions.JOOLS):F2} jools.")
        item.Remove()
    End Sub

    Private Sub HandleUnmoldCake(verb As IVerb, item As IItem, actor As ICharacter)
        Dim cakePan = actor.Inventory.GetItemsOfSubtype(ItemSubtypes.CAKE_PAN).FirstOrDefault()
        actor.AddMessage($"{actor.Name} unmolds {cakePan.Name} onto {item.Name}.")
        item.ChangeCounter(Counters.LAYERS, 1)
        cakePan.ClearTag(Tags.CAKE)
        actor.AddMessage($"{item.Name} how has a {item.GetCounter(Counters.LAYERS)} layer cake.")
    End Sub

    Private Sub HandlePourBatter(verb As IVerb, item As IItem, actor As ICharacter)
        Dim mixingBowl = actor.Inventory.GetItemsOfSubtype(ItemSubtypes.MIXING_BOWL).FirstOrDefault()
        Dim batter = mixingBowl.GetDimension(Dimensions.BATTER)
        actor.AddMessage($"{actor.Name} pours {batter:f2} batter from {mixingBowl.Name} to {item.Name}.")
        mixingBowl.ChangeDimension(Dimensions.BATTER, -batter)
        item.ChangeDimension(Dimensions.BATTER, batter)
    End Sub

    Private Sub HandleMix(verb As IVerb, item As IItem, actor As ICharacter)
        actor.AddMessage($"{actor.Name} mixes the ingredients in {item.Name}.")
        item.Mix()
        actor.DoBiology(1)
    End Sub

    <Extension>
    Sub Perform(verb As IVerb, item As IItem, actor As ICharacter)
        Dim handler As PerformHandler = Nothing
        If performTable.TryGetValue(verb.EntitySubtype, handler) Then
            handler.Invoke(verb, item, actor)
            Return
        End If
    End Sub
#End Region
End Module
