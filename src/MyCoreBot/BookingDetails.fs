namespace MyCoreBot

type BookingDetails () =
    member val Destination: string  = null with get, set
    member val Origin: string       = null with get, set
    member val TravelDate: string   = null with get, set