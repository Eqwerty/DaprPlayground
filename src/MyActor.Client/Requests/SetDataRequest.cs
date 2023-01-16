namespace MyActor.Client.Requests;

public record SetDataRequest(
    string User,
    string PropertyA,
    string PropertyB
);