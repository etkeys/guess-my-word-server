
using System.Reflection.Metadata.Ecma335;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GmwServer;

public class DefinitionConfiguration: IEntityTypeConfiguration<Definition>
{
    public void Configure(EntityTypeBuilder<Definition> builder){
        builder.HasData(
            new {LiteralWord = "steak", WordDefinitionId = 1, DefinitionText = "a slice of meat cut from a fleshly part of a beef carcass"},
            new {LiteralWord = "steak", WordDefinitionId = 2, DefinitionText = "a similar slice of a specified meat other than beef"},
            new {LiteralWord = "steak", WordDefinitionId = 3, DefinitionText = "a cross-section slice of a large fish"},
            new {LiteralWord = "steak", WordDefinitionId = 4, DefinitionText = "a thick slice or piece of a non-meat food especially when prepared or served in the manner of a beef steak"},
            new {LiteralWord = "steak", WordDefinitionId = 5, DefinitionText = "ground beef for cooking or for service in the manner of a steak"},
            new {LiteralWord = "steak", WordDefinitionId = 6, DefinitionText = "a non-meat food formed into a patter and cooked"},

            new {LiteralWord = "media", WordDefinitionId = 1, DefinitionText = "a medium of cultivation, conveyance, or expression"},

            new {LiteralWord = "variety", WordDefinitionId = 1, DefinitionText = "the quality of state of having different forms or types"},
            new {LiteralWord = "variety", WordDefinitionId = 2, DefinitionText = "a number or collection of diffrent things especially of a particular class"},
            new {LiteralWord = "variety", WordDefinitionId = 3, DefinitionText = "something differening from others of the same general kind"},
            new {LiteralWord = "variety", WordDefinitionId = 4, DefinitionText = "any various groups of plants or animals ranking below a species"},

            new {LiteralWord = "skill", WordDefinitionId = 1, DefinitionText = "the ability to use one's knowledge effectively and readily in execution or performance"},
            new {LiteralWord = "skill", WordDefinitionId = 2, DefinitionText = "dexterity or coordication especially in the execution of learned physical tasks"},
            new {LiteralWord = "skill", WordDefinitionId = 3, DefinitionText = "a learned power of doing something compentently"},

            new {LiteralWord = "family", WordDefinitionId = 1, DefinitionText = "the basic unit in society traditionally consiting of two parents reaing their children"},
            new {LiteralWord = "family", WordDefinitionId = 2, DefinitionText = "spouse and children"},
            new {LiteralWord = "family", WordDefinitionId = 3, DefinitionText = "a group of individuals living under one roof and usually one head"},
            new {LiteralWord = "family", WordDefinitionId = 4, DefinitionText = "a group of persons of common ancestry"},
            new {LiteralWord = "family", WordDefinitionId = 5, DefinitionText = "a people or group of peoples regarded as deriving from a common stock"},
            new {LiteralWord = "family", WordDefinitionId = 6, DefinitionText = "a group of people united by certain convictions or a common affiliation"},
            new {LiteralWord = "family", WordDefinitionId = 7, DefinitionText = "the staff of a high official (such as the President)"},
            new {LiteralWord = "family", WordDefinitionId = 8, DefinitionText = "a group of things related by common chracteristics"},

            new {LiteralWord = "embrace", WordDefinitionId = 1, DefinitionText = "to clasp in arms"},
            new {LiteralWord = "embrace", WordDefinitionId = 2, DefinitionText = "cherish, love"},
            new {LiteralWord = "embrace", WordDefinitionId = 3, DefinitionText = "encircle, enclose"},
            new {LiteralWord = "embrace", WordDefinitionId = 4, DefinitionText = "to take up especially readily or gladly"},
            new {LiteralWord = "embrace", WordDefinitionId = 5, DefinitionText = "to avail onself of"},
            new {LiteralWord = "embrace", WordDefinitionId = 6, DefinitionText = "to take in or include as part, item, or element of a more inclusive whole"},
            new {LiteralWord = "embrace", WordDefinitionId = 7, DefinitionText = "to be equal or equivalent to"},

            new {LiteralWord = "welcome", WordDefinitionId = 1, DefinitionText = "to greet hospitably and with courtesy or cordiality"},
            new {LiteralWord = "welcome", WordDefinitionId = 2, DefinitionText = "to accept with pleasure or present of"},

            new {LiteralWord = "give", WordDefinitionId = 1, DefinitionText = "to make a present of"},
            new {LiteralWord = "give", WordDefinitionId = 2, DefinitionText = "to grant or bestow by formal action"},
            new {LiteralWord = "give", WordDefinitionId = 3, DefinitionText = "to accord or yield to another"},
            new {LiteralWord = "give", WordDefinitionId = 4, DefinitionText = "to put into the possession of another for his or her use"},
            new {LiteralWord = "give", WordDefinitionId = 5, DefinitionText = "to administor as a sacrament"},

            new {LiteralWord = "sell", WordDefinitionId = 1, DefinitionText = "to deliver or give up in violation of duty, trust, or loyalty and especially for personal gain"},
            new {LiteralWord = "sell", WordDefinitionId = 2, DefinitionText = "to give up (property) to another for something of value (such as money)"},
            new {LiteralWord = "sell", WordDefinitionId = 3, DefinitionText = "to give up in return for something else especially foolishly or dishonorably"},

            new {LiteralWord = "point", WordDefinitionId = 1, DefinitionText = "an individual detail"},
            new {LiteralWord = "point", WordDefinitionId = 2, DefinitionText = "the most important essential in a discussion or matter"}

        );
    }
}