import { Injectable, signal } from "@angular/core";
import { Router } from "@angular/router";

@Injectable({providedIn: 'root'})
export class ServiceAuth {
    // tipo de valor y el valor que le asignamos
    public logged = signal<boolean>(false)
    constructor(
        private _router: Router
    ){
        this.checkInitialState()
    }

    private checkInitialState(): void {
        const token = sessionStorage.getItem('Token')
        if (token) {
            this.logged.set(true)
        }
    }

    logIn(token: string): void {
        sessionStorage.setItem('Token',token)
        this.logged.set(true)
    }

    logOut(): void {
        sessionStorage.removeItem("Token");
        this.logged.set(false)
    }
}