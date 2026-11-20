import { ChangeDetectorRef, Component, ElementRef, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { Usuario } from '../../models/usuario';
import { ServiceLogin } from '../../services/service.login';

@Component({
  selector: 'app-login-component',
  standalone: false,
  styleUrl: './login-component.css',
  templateUrl: './login-component.html',
})
export class LoginComponent {
  @ViewChild("cajaUsuario") cajaUser!: ElementRef;
  @ViewChild("cajaContraseña") cajaContraseña!: ElementRef;
  public mensaje!: string;

  constructor(
    private _cdr: ChangeDetectorRef,
    private _service: ServiceLogin,
    private _router: Router
  ){}

  async logIn(): Promise<void>{
    let usuario = this.cajaUser.nativeElement.value;
    let contraseña = this.cajaContraseña.nativeElement.value;
    let login = new Usuario(usuario, contraseña);
    let response = await this._service.logIn(login);
    this.mensaje = response
    this._router.navigate(['/'])
    this._cdr.detectChanges();
  }
}
