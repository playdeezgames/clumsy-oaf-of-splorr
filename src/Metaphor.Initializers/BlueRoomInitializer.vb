Imports Metaphor.Extensions
Imports Metaphor.Persistence

Friend Module BlueRoomInitializer
    Friend Function Initialize(context As IInitializationContext) As Persistence.LocationInitializer
        Return Sub(room)
                   room.CreateN00b(context.ChosenName, InitializeAvatar(context))
                   room.CreateDoor("Door to Bakery", InitializeBakeryDoor(context))
                   room.CreateBed(InitializeBed(context))
               End Sub
    End Function
    Private Function InitializeBed(context As IInitializationContext) As FeatureInitializer
        Return Sub(bed)
                   bed.CreateSleepVerb()
               End Sub
    End Function
    Private Function InitializeBakeryDoor(context As IInitializationContext) As FeatureInitializer
        Return Sub(feature)
                   Dim bakery = feature.World.CreateLocation(
                        LocationSubtypes.BAKERY,
                        "The Aggressively Pink Bakery",
                        BakeryInitializer.Initialize(context, feature.Location))
                   feature.SetDestination(bakery)
                   feature.CreateEnterVerb()
               End Sub
    End Function

    Private Function InitializeAvatar(context As IInitializationContext) As CharacterInitializer
        Return Sub(character)
                   character.World.Avatar = character
#If DEBUG Then
                   character.InitializeDimension(Dimensions.JOOLS, 20.0, 0.0, Double.MaxValue)
                   character.InitializeCounter(Counters.ENERGY, 10, 0, 100)
                   character.InitializeCounter(Counters.HEALTH, 10, 0, 100)
#Else
                   character.InitializeDimension(Dimensions.JOOLS, 0.0, 0.0, Double.MaxValue)
                   character.InitializeCounter(Counters.ENERGY, 100, 0, 100)
                   character.InitializeCounter(Counters.HEALTH, 100, 0, 100)
#End If
               End Sub
    End Function
End Module
