
using System.Reflection.Metadata.Ecma335;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GmwServer;

public class DefinitionConfiguration: IEntityTypeConfiguration<Definition>
{
    public void Configure(EntityTypeBuilder<Definition> builder){
        builder.HasData(
            new {Id = DefinitionId.FromString("210d9d1f-c997-4983-be13-5949aaf405e8"), LiteralWord = "steak", WordDefinitionId = 1, DefinitionText = "a slice of meat cut from a fleshly part of a beef carcass"},
            new {Id = DefinitionId.FromString("02b7e058-ea0f-4a44-bea0-e6cad197b951"), LiteralWord = "steak", WordDefinitionId = 2, DefinitionText = "a similar slice of a specified meat other than beef"},
            new {Id = DefinitionId.FromString("9682c6f0-b1cc-48d4-9de3-91e5c79afbeb"), LiteralWord = "steak", WordDefinitionId = 3, DefinitionText = "a cross-section slice of a large fish"},
            new {Id = DefinitionId.FromString("fc8b390c-025a-4d9a-b981-e93f4a87665c"), LiteralWord = "steak", WordDefinitionId = 4, DefinitionText = "a thick slice or piece of a non-meat food especially when prepared or served in the manner of a beef steak"},
            new {Id = DefinitionId.FromString("03c2f907-21cb-4aa0-bb6e-20fa264e2514"), LiteralWord = "steak", WordDefinitionId = 5, DefinitionText = "ground beef for cooking or for service in the manner of a steak"},
            new {Id = DefinitionId.FromString("928ac102-3c0c-4c98-a431-7d2ce68a0c97"), LiteralWord = "steak", WordDefinitionId = 6, DefinitionText = "a non-meat food formed into a patter and cooked"},

            new {Id = DefinitionId.FromString("2dc96c83-3329-48fe-9677-e9eb9f443b38"), LiteralWord = "media", WordDefinitionId = 1, DefinitionText = "a medium of cultivation, conveyance, or expression"},

            new {Id = DefinitionId.FromString("f3ac0be5-f606-4162-98a5-0db1e12c5ccc"), LiteralWord = "variety", WordDefinitionId = 1, DefinitionText = "the quality of state of having different forms or types"},
            new {Id = DefinitionId.FromString("f61386d3-0086-4ca8-b1b7-12ba4c7dfaa6"), LiteralWord = "variety", WordDefinitionId = 2, DefinitionText = "a number or collection of diffrent things especially of a particular class"},
            new {Id = DefinitionId.FromString("0172fead-c3ed-4f8b-b64c-bbbbb873777b"), LiteralWord = "variety", WordDefinitionId = 3, DefinitionText = "something differening from others of the same general kind"},
            new {Id = DefinitionId.FromString("36b2ce44-aa40-432d-8c66-05a9cc2c2cce"), LiteralWord = "variety", WordDefinitionId = 4, DefinitionText = "any various groups of plants or animals ranking below a species"},

            new {Id = DefinitionId.FromString("5e911a3e-a1a5-4ff7-9030-42ec60421a6b"), LiteralWord = "skill", WordDefinitionId = 1, DefinitionText = "the ability to use one's knowledge effectively and readily in execution or performance"},
            new {Id = DefinitionId.FromString("c4b28ddc-d912-4bc2-8765-5c3c1e27a6bb"), LiteralWord = "skill", WordDefinitionId = 2, DefinitionText = "dexterity or coordication especially in the execution of learned physical tasks"},
            new {Id = DefinitionId.FromString("11a98d1d-f12b-4a18-ad93-4c7257ff784d"), LiteralWord = "skill", WordDefinitionId = 3, DefinitionText = "a learned power of doing something compentently"},

            new {Id = DefinitionId.FromString("dd026f8e-4fd2-4243-8540-b28c918fd1d9"), LiteralWord = "family", WordDefinitionId = 1, DefinitionText = "the basic unit in society traditionally consiting of two parents reaing their children"},
            new {Id = DefinitionId.FromString("9b91590a-79bc-43e4-8640-c995c4d05c10"), LiteralWord = "family", WordDefinitionId = 2, DefinitionText = "spouse and children"},
            new {Id = DefinitionId.FromString("ec511faf-77e0-400d-9a89-5a30dc107fa5"), LiteralWord = "family", WordDefinitionId = 3, DefinitionText = "a group of individuals living under one roof and usually one head"},
            new {Id = DefinitionId.FromString("3a7ae82d-6662-4174-b39b-add6c8be6797"), LiteralWord = "family", WordDefinitionId = 4, DefinitionText = "a group of persons of common ancestry"},
            new {Id = DefinitionId.FromString("d93e85d3-0e7e-4a1f-80bc-75b7761a3606"), LiteralWord = "family", WordDefinitionId = 5, DefinitionText = "a people or group of peoples regarded as deriving from a common stock"},
            new {Id = DefinitionId.FromString("4b67c7b9-bd57-499f-82df-8b76a692d3ca"), LiteralWord = "family", WordDefinitionId = 6, DefinitionText = "a group of people united by certain convictions or a common affiliation"},
            new {Id = DefinitionId.FromString("9ab99839-3335-4211-94ea-6a05865577fd"), LiteralWord = "family", WordDefinitionId = 7, DefinitionText = "the staff of a high official (such as the President)"},
            new {Id = DefinitionId.FromString("0af4c005-aa12-49c9-a24c-0f8986dacfbd"), LiteralWord = "family", WordDefinitionId = 8, DefinitionText = "a group of things related by common chracteristics"},

            new {Id = DefinitionId.FromString("f6145eba-2e93-46d1-a53d-7d7ceaf106e3"), LiteralWord = "embrace", WordDefinitionId = 1, DefinitionText = "to clasp in arms"},
            new {Id = DefinitionId.FromString("d86b4dad-d34f-466c-9271-28c49191aefc"), LiteralWord = "embrace", WordDefinitionId = 2, DefinitionText = "cherish, love"},
            new {Id = DefinitionId.FromString("9ffe8dae-8946-4adc-ae4a-d8be09012d68"), LiteralWord = "embrace", WordDefinitionId = 3, DefinitionText = "encircle, enclose"},
            new {Id = DefinitionId.FromString("52608a26-8edd-4e8d-b907-ff7b561a0407"), LiteralWord = "embrace", WordDefinitionId = 4, DefinitionText = "to take up especially readily or gladly"},
            new {Id = DefinitionId.FromString("e811debc-b359-470d-ba18-9749c1a627f6"), LiteralWord = "embrace", WordDefinitionId = 5, DefinitionText = "to avail onself of"},
            new {Id = DefinitionId.FromString("c0e9ac26-b836-4d31-b00e-6e79f322798f"), LiteralWord = "embrace", WordDefinitionId = 6, DefinitionText = "to take in or include as part, item, or element of a more inclusive whole"},
            new {Id = DefinitionId.FromString("e54068b3-56b4-44bf-8f90-c161ab7f1286"), LiteralWord = "embrace", WordDefinitionId = 7, DefinitionText = "to be equal or equivalent to"},

            new {Id = DefinitionId.FromString("93f0d74e-c12b-4f20-bdf2-9cbfc8e97d1a"), LiteralWord = "welcome", WordDefinitionId = 1, DefinitionText = "to greet hospitably and with courtesy or cordiality"},
            new {Id = DefinitionId.FromString("38d9714b-adcb-42f8-ac8a-99b923e26283"), LiteralWord = "welcome", WordDefinitionId = 2, DefinitionText = "to accept with pleasure or present of"},

            new {Id = DefinitionId.FromString("e131ce3c-35f8-4e30-b06c-01b4673a7418"), LiteralWord = "give", WordDefinitionId = 1, DefinitionText = "to make a present of"},
            new {Id = DefinitionId.FromString("9d1249ba-b131-44d7-a9ee-c9cfb93edbda"), LiteralWord = "give", WordDefinitionId = 2, DefinitionText = "to grant or bestow by formal action"},
            new {Id = DefinitionId.FromString("707f7074-8ad2-401a-b61c-9b42e69bdc2d"), LiteralWord = "give", WordDefinitionId = 3, DefinitionText = "to accord or yield to another"},
            new {Id = DefinitionId.FromString("07a8b3e3-d79b-4e37-97f1-a456f0d9e85f"), LiteralWord = "give", WordDefinitionId = 4, DefinitionText = "to put into the possession of another for his or her use"},
            new {Id = DefinitionId.FromString("0ca26e87-80c2-4366-8f6a-2d42f026639f"), LiteralWord = "give", WordDefinitionId = 5, DefinitionText = "to administor as a sacrament"},

            new {Id = DefinitionId.FromString("5ded73a7-7d5f-4da2-8cfa-67e8d9662967"), LiteralWord = "sell", WordDefinitionId = 1, DefinitionText = "to deliver or give up in violation of duty, trust, or loyalty and especially for personal gain"},
            new {Id = DefinitionId.FromString("b18bfc54-4e85-4c9b-8f92-fd63f414e496"), LiteralWord = "sell", WordDefinitionId = 2, DefinitionText = "to give up (property) to another for something of value (such as money)"},
            new {Id = DefinitionId.FromString("24502a3e-9c13-4933-ab5b-6418ba31b72c"), LiteralWord = "sell", WordDefinitionId = 3, DefinitionText = "to give up in return for something else especially foolishly or dishonorably"},

            new {Id = DefinitionId.FromString("ad0966c4-652d-4cec-aac9-dae32db80c31"), LiteralWord = "point", WordDefinitionId = 1, DefinitionText = "an individual detail"},
            new {Id = DefinitionId.FromString("51cb204f-db1b-4c2f-8a52-98ffdafc09c1"), LiteralWord = "point", WordDefinitionId = 2, DefinitionText = "the most important essential in a discussion or matter"}

        );
    }
}