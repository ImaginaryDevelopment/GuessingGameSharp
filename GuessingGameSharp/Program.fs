﻿open System
open System.Collections.Generic

type GameTermination =
    | Won
    | GaveUp

type GameState = {
    Guesses:Set<int>
    Target:int
    Count:int
    Termination: GameTermination option
}

let (|Parse|_|) (str: string) : int option =
    match Int32.TryParse(str) with
    | true, value -> Some value
    | _ -> None

let parse = 
  function
  | Parse v -> v
  | v -> failwithf "Argument provided was not an int: '%s'" v

let clamp (lower,upper) x = 
  if x < lower then failwithf "Argument must be greater than or equal to %i but was %i" lower x
  elif x > upper then failwithf "Argument must be less than or equal to %i but was %i" upper x
  else x

let parseRange args =
    Array.tryHead args
    |> Option.map (parse>>clamp (1,350))
    |> function
        | None -> failwith "Range was not provided"
        | Some x -> x

let runGame range : (GameState * GameState) option =
    let random = Random()
    let number = random.Next(1, range+1)
    Console.WriteLine $"OK, let's start. I am thinking of a number between 1 to {range}"
    Console.WriteLine "Enter your guess: "
    let mutable guess = 0
    let mutable gameState = 
        {
            Guesses = Set.empty
            Target = number
            Count = 0
            Termination = None
        }
    gameState |> Seq.unfold (fun game -> 
                let userInput = Console.ReadLine()
                match userInput with
                | Parse input ->
                    guess <- input
                    if gameState.Guesses.Contains guess then
                        Console.WriteLine "You already guessed that!!"
                        Some (gameState, gameState)
                    elif 0 >= guess || guess > range then
                        Console.WriteLine $"Sorry, {guess} is less than or equal to zero or greater than {range}."
                        gameState <- { gameState with Count = gameState.Count - 1 }
                        Some (gameState, gameState)
                    elif guess < gameState.Target then
                        Console.WriteLine $"{guess} is too low!"
                        Some (gameState, gameState)
                    elif guess > gameState.Target then
                        Console.WriteLine $"{guess} is too high!"
                        Some (gameState, gameState)
                    else
                        gameState <- { gameState with Termination = Some Won }
                        None
                    gameState.Guesses.Add(guess)
                    gameState <- { gameState with Count = gameState.Count + 1 }
                    Console.WriteLine "Enter your guess: "
                | _ as input ->
                    if input.Equals "give up" then
                        gameState <- { gameState with Termination = Some GaveUp }
                        None
                    else
                        Console.WriteLine $"\"{userInput}\" should be an integer: "
                        Some (gameState, gameState)
                )
            |> Seq.last

let displayEndState (hasWon, number, count) =
    match hasWon with
    | true ->
        Console.Clear()
        Console.WriteLine $"Yay! you guessed my number!! It took you {count} tries! (Not counting bad inputs.)"
    | _ ->
        Console.Clear()
        Console.WriteLine $":( You gave up. The number was {number}. It took you {count} tries. (Not counting bad inputs.)"

[<EntryPoint>]
let main args =
    args
    |> parseRange
    |> runGame
    |> displayEndState
    0