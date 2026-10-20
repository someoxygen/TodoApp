export interface RegisterRequest{
    fullName:string
    email:string
    password:string
    confirmPassword:string
}

export interface LoginRequest{
    email:string
    password:string
}
