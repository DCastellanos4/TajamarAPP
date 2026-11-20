import { Injectable } from "@angular/core";
import { environment } from "../../environments/environment.development";
import { HttpClient, HttpContext, HttpHeaders } from "@angular/common/http";
import { ServiceAuth } from "./service.auth";
import { Router } from "@angular/router";
import { firstValueFrom } from "rxjs";
import { Usuario } from "../models/usuario";
import { SKIP_TOKEN } from "../utils/token.context";

@Injectable()
export class ServiceLogin {
    urlAlumnos = environment.apiAlumnos
    constructor(
        private _http: HttpClient,
        private _authState: ServiceAuth,
        private _router: Router
    ){}

    async logIn(usu: Usuario): Promise<any> {
        let request = "api/manage/login"
        let json = JSON.stringify(usu)
        let header = new HttpHeaders()
            .set("content-type", "application/json")
        let response = await firstValueFrom(
            this._http.post<any>(this.urlAlumnos+request, json, {
                headers: header,
                context: new HttpContext().set(SKIP_TOKEN, true)
            })
        )
        if(response && response.response) {
            this._authState.logIn(response.response)
        }
        return response
    }
}