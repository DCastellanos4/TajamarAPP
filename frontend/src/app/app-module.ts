import { NgModule, provideBrowserGlobalErrorListeners } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { AppRoutingModule } from './app-routing-module';
import { App } from './app';
import { LoginComponent } from './components/login-component/login-component';
import { HomeComponent } from './components/home-component/home-component';
import { provideHttpClient } from '@angular/common/http';
import { ServiceLogin } from './services/service.login';
import { AuthGuard } from './guards/auth-guard';

@NgModule({
  declarations: [App, LoginComponent, HomeComponent],
  imports: [BrowserModule, AppRoutingModule],
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideHttpClient(),
    ServiceLogin,
    AuthGuard
  ],
  bootstrap: [App],
})
export class AppModule {}
